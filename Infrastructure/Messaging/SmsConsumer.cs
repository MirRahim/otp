using MassTransit;
using OtpSystem.Application.Interfaces;
using OtpSystem.Application.Messaging;

namespace OtpSystem.Infrastructure.Messaging;

public class SmsConsumer : IConsumer<SendOtpMessage>
{
    private readonly ISmsService _smsService;
    private readonly IWebhookService _webhook;
    private readonly ILogger<SmsConsumer> _logger;

    public SmsConsumer(
        ISmsService smsService,
        IWebhookService webhook,
        ILogger<SmsConsumer> logger)
    {
        _smsService = smsService;
        _webhook = webhook;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendOtpMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OTP SMS for {TenantId}:{Phone}",
            message.TenantId, message.Phone);

        var sent = await _smsService.SendOtpAsync(message.Phone, message.Code);

        // fire webhook regardless of success/failure
        if (!string.IsNullOrEmpty(message.WebhookUrl))
        {
            await _webhook.SendAsync(message.WebhookUrl, new WebhookPayload
            {
                Event = sent ? "otp.sent" : "otp.failed",
                TenantId = message.TenantId,
                Phone = message.Phone,
                Success = sent,
                Timestamp = DateTime.UtcNow
            });
        }

        if (!sent)
        {
            _logger.LogError(
                "Failed to send OTP for {TenantId}:{Phone}",
                message.TenantId, message.Phone);

            // throw so MassTransit retries + eventually DLQs
            throw new Exception($"SMS send failed for {message.Phone}");
        }
    }
}