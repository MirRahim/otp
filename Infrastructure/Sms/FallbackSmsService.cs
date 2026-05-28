using OtpSystem.Application.Interfaces;

namespace OtpSystem.Infrastructure.Sms;

public class FallbackSmsService : ISmsService
{
    private readonly ILogger<FallbackSmsService> _logger;

    public FallbackSmsService(ILogger<FallbackSmsService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string phone, string code)
    {
        // Replace with your actual fallback provider (Kavenegar, Twilio, etc.)
        _logger.LogWarning("FallbackSmsService used for {Phone}", phone);

        // TODO: call fallback provider API here
        await Task.Delay(100); // simulate async call
        return true;
    }
}