using OtpSystem.Application.Interfaces;

namespace OtpSystem.API.Middlewares;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private const string IdempotencyHeader = "Idempotency-Key";

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cache)
    {
        // Only apply to POST requests
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        // If no idempotency key, just continue
        if (!context.Request.Headers.TryGetValue(IdempotencyHeader, out var key))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"idempotency:{key}";
        var cached = await cache.GetAsync(cacheKey);

        // Already processed — return cached response
        if (cached is not null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cached);
            return;
        }

        // Intercept the response so we can cache it
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        memStream.Position = 0;
        var responseBody = await new StreamReader(memStream).ReadToEndAsync();

        // Only cache successful responses
        if (context.Response.StatusCode is >= 200 and < 300)
            await cache.SetAsync(cacheKey, responseBody, TimeSpan.FromMinutes(5));

        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
}