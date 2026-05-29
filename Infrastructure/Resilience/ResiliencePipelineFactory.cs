using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace OtpSystem.Infrastructure.Resilience;

public static class ResiliencePipelineFactory
{
    public static IHttpResiliencePipelineBuilder AddSmsResiliencePipeline(
        this IHttpClientBuilder builder)
    {
        return builder.AddResilienceHandler("sms-pipeline", pipeline =>
        {
            // 1. Retry — try 3 times with exponential backoff
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(300),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: HttpRequestException } => PredicateResult.True(),
                    { Result.IsSuccessStatusCode: false } => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"[Retry] Attempt {args.AttemptNumber + 1} after {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            });

            // 2. Circuit Breaker — open after 3 failures, stay open for 30s
            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                FailureRatio = 0.5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = args => args.Outcome switch
                {
                    { Exception: HttpRequestException } => PredicateResult.True(),
                    { Result.IsSuccessStatusCode: false } => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                OnOpened = args =>
                {
                    Console.WriteLine(
                        $"[CircuitBreaker] OPEN — SMS provider down for {args.BreakDuration.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    Console.WriteLine("[CircuitBreaker] CLOSED — SMS provider recovered");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    Console.WriteLine("[CircuitBreaker] HALF-OPEN — testing SMS provider");
                    return ValueTask.CompletedTask;
                }
            });

            // 3. Timeout — don't wait more than 5s per attempt
            pipeline.AddTimeout(TimeSpan.FromSeconds(5));
        });
    }
}