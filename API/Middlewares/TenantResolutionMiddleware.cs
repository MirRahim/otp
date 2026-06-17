using OtpSystem.Application.Interfaces;
using OtpSystem.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace OtpSystem.API.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-Api-Key";

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantRepository tenantRepository,
        TenantContext tenantContext)
    {
        // skip health checks and swagger
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/health") || path.StartsWith("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var rawKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "TENANT_001",
                message = "X-Api-Key header is required"
            });
            return;
        }

        var hashedKey = HashApiKey(rawKey!);
        var tenant = await tenantRepository.GetByApiKeyAsync(hashedKey);

        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "TENANT_002",
                message = "Invalid or inactive API key"
            });
            return;
        }

        tenantContext.Set(tenant.Id, tenant.Name, tenant.WebhookUrl);
        await _next(context);
    }

    public static string HashApiKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}