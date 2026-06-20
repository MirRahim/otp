using OtpSystem.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace OtpSystem.Infrastructure.Webhook;

public class WebhookService : IWebhookService
{
    private readonly HttpClient _http;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(HttpClient http, ILogger<WebhookService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task SendAsync(string webhookUrl, WebhookPayload payload)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync(webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Webhook failed for {TenantId}. Status: {Status}",
                    payload.TenantId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            // webhook failure must never affect OTP flow
            _logger.LogError(ex, "Webhook threw exception for {TenantId}", payload.TenantId);
        }
    }
}