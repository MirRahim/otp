using MassTransit;
using OtpSystem.Application.Interfaces;
using OtpSystem.Application.Messaging;

namespace OtpSystem.Infrastructure.Messaging;

public class SmsConsumer : IConsumer<SendOtpMessage>
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsConsumer> _logger;

    public SmsConsumer(ISmsService smsService, ILogger<SmsConsumer> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendOtpMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing OTP SMS for {Phone}", message.Phone);

        var sent = await _smsService.SendOtpAsync(message.Phone, message.Code);

        if (!sent)
        {
            _logger.LogError("Failed to send OTP SMS for {Phone} after all retries", message.Phone);

            // MassTransit will retry based on retry policy below
            throw new Exception($"SMS send failed for {message.Phone}");
        }

        _logger.LogInformation("OTP SMS sent successfully for {Phone}", message.Phone);
    }
}