using Microsoft.Extensions.Diagnostics.HealthChecks;
using OtpSystem.Application.Interfaces;

namespace OtpSystem.API.HealthChecks;

public class OtpServiceHealthCheck : IHealthCheck
{
    private readonly ICacheService _cache;

    public OtpServiceHealthCheck(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var testKey = "health:otp-ping";
            await _cache.SetAsync(testKey, "ok", TimeSpan.FromSeconds(5));
            var value = await _cache.GetAsync(testKey);

            if (value != "ok")
                return HealthCheckResult.Degraded("Cache read/write mismatch");

            return HealthCheckResult.Healthy("OTP cache is operational");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("OTP cache failed", ex);
        }
    }
}