using OtpSystem.Application.Interfaces;

namespace OtpSystem.Infrastructure.Sms;

public class SmsSenderService : ISmsService
{
    private readonly SmsIrService _primary;
    private readonly FallbackSmsService _fallback;
    private readonly ILogger<SmsSenderService> _logger;

    public SmsSenderService(
        SmsIrService primary,
        FallbackSmsService fallback,
        ILogger<SmsSenderService> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string phone, string code)
    {
        var sent = await _primary.SendOtpAsync(phone, code);

        if (!sent)
        {
            _logger.LogWarning("Primary SMS failed, switching to fallback for {Phone}", phone);
            sent = await _fallback.SendOtpAsync(phone, code);
        }

        return sent;
    }
}