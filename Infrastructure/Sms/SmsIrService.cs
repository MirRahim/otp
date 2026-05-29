using OtpSystem.Application.Interfaces;
using Polly.CircuitBreaker;
using System.Text;
using System.Text.Json;

namespace OtpSystem.Infrastructure.Sms;

public class SmsIrService : ISmsService
{
    private readonly HttpClient _http;
    private readonly ILogger<SmsIrService> _logger;

    public SmsIrService(HttpClient http, ILogger<SmsIrService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string phone, string code)
    {
        var payload = new
        {
            Mobile = phone,
            TemplateId = 347022,
            Parameters = new[] { new { Name = "CODE", Value = code } }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _http.PostAsync("/v1/send/verify", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SmsIr failed for {Phone}. Status: {Status}",
                    phone, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open — don't even try, go straight to fallback
            _logger.LogWarning("Circuit is OPEN — skipping SmsIr for {Phone}", phone);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SmsIr threw exception for {Phone}", phone);
            return false;
        }
    }
}