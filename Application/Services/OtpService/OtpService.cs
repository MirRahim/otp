using OtpSystem.Application.Common;
using OtpSystem.Application.Interfaces;
using OtpSystem.Application.Messaging;
using OtpSystem.Domain.Entities;
using OtpSystem.Domain.Enums;
using OtpSystem.Infrastructure.Persistence;

namespace OtpSystem.Application.Services.OtpService;

public class OtpService : IOtpService
{
    private readonly ICacheService _cache;
    private readonly IMessagePublisher _publisher;
    private readonly IOtpRepository _otpRepository;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<OtpService> _logger;

    private const string OtpKeyPrefix = "otp:";
    private const string RateKeyPrefix = "rate:";
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(2);

    public OtpService(
        ICacheService cache,
        IMessagePublisher publisher,
        IOtpRepository otpRepository,
        TenantContext tenantContext,
        ILogger<OtpService> logger)
    {
        _cache = cache;
        _publisher = publisher;
        _otpRepository = otpRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> SendOtpAsync(string phone)
    {
        var tenantId = _tenantContext.TenantId;

        // rate limit per tenant+phone
        var rateKey = $"{RateKeyPrefix}{tenantId}:{phone}";
        var count = await _cache.IncrementAsync(rateKey, TimeSpan.FromMinutes(1));
        if (count > 3)
        {
            _logger.LogWarning("Rate limit exceeded for {TenantId}:{Phone}", tenantId, phone);
            return Result.Failure(OtpErrors.TooManyRequests, "Too many requests");
        }

        var code = Random.Shared.Next(100000, 999999).ToString();

        // persist to DB — source of truth
        var record = OtpRecord.Create(tenantId, phone, code, OtpTtl);
        await _otpRepository.AddAsync(record);

        // cache in Redis — fast verify
        var otpKey = $"{OtpKeyPrefix}{tenantId}:{phone}";
        await _cache.SetAsync(otpKey, code, OtpTtl);

        // publish to queue — async SMS delivery
        await _publisher.PublishAsync(new SendOtpMessage
        {
            Phone = phone,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            WebhookUrl = _tenantContext.WebhookUrl
        });

        _logger.LogInformation("OTP queued for {TenantId}:{Phone}", tenantId, phone);
        return Result.Success();
    }

    public async Task<Result<bool>> VerifyOtpAsync(string phone, string code)
    {
        var tenantId = _tenantContext.TenantId;
        var otpKey = $"{OtpKeyPrefix}{tenantId}:{phone}";

        // fast path — Redis
        var cached = await _cache.GetAsync(otpKey);
        if (cached is not null)
        {
            if (cached != code)
                return Result<bool>.Failure(OtpErrors.InvalidCode, "Invalid code");

            await _cache.DeleteAsync(otpKey);

            // update DB in background
            _ = Task.Run(async () =>
            {
                var record = await _otpRepository.GetPendingAsync(tenantId, phone);
                if (record is not null)
                {
                    record.Verify(code);
                    await _otpRepository.UpdateAsync(record);
                }
            });

            return Result<bool>.Success(true);
        }

        // fallback — DB (Redis miss: restart, eviction)
        var dbRecord = await _otpRepository.GetPendingAsync(tenantId, phone);
        if (dbRecord is null)
            return Result<bool>.Failure(OtpErrors.NotFound, "OTP not found or expired");

        var result = dbRecord.Verify(code);
        await _otpRepository.UpdateAsync(dbRecord);

        return result == OtpVerifyResult.Success
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(OtpErrors.InvalidCode, result.ToString());
    }
}