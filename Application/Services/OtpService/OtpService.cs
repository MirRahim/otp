using OtpSystem.Application.Common;
using OtpSystem.Application.Interfaces;
using OtpSystem.Application.Messaging;
using OtpSystem.Domain.Entities;
using OtpSystem.Domain.Enums;

namespace OtpSystem.Application.Services.OtpService;

public class OtpService : IOtpService
{
    private readonly ICacheService _cache;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<OtpService> _logger;

    private const string OtpKeyPrefix = "otp:";
    private const string RateKeyPrefix = "rate:";

    public OtpService(
        ICacheService cache,
        IMessagePublisher publisher,
        ILogger<OtpService> logger)
    {
        _cache = cache;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result> SendOtpAsync(string phone)
    {
        // Rate limit check
        var rateKey = $"{RateKeyPrefix}{phone}";
        var count = await _cache.IncrementAsync(rateKey, TimeSpan.FromMinutes(1));

        if (count > 3)
        {
            _logger.LogWarning("Rate limit exceeded for {Phone}", phone);
            return Result.Failure(OtpErrors.TooManyRequests, "Too many requests");
        }

        // Generate and store OTP
        var code = Random.Shared.Next(100000, 999999).ToString();
        var otpKey = $"{OtpKeyPrefix}{phone}";
        await _cache.SetAsync(otpKey, code, TimeSpan.FromMinutes(2));

        // Publish to queue — don't wait for SMS to be sent
        await _publisher.PublishAsync(new SendOtpMessage
        {
            Phone = phone,
            Code = code,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("OTP queued for {Phone}", phone);
        return Result.Success();
    }

    public async Task<Result<bool>> VerifyOtpAsync(string phone, string code)
    {
        var otpKey = $"{OtpKeyPrefix}{phone}";
        var stored = await _cache.GetAsync(otpKey);

        if (stored is null)
            return Result<bool>.Failure(OtpErrors.NotFound, "OTP not found or expired");

        if (stored != code)
            return Result<bool>.Failure(OtpErrors.InvalidCode, "Invalid code");

        await _cache.DeleteAsync(otpKey);
        return Result<bool>.Success(true);
    }
}