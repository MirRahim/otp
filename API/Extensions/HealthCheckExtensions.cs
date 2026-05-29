using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OtpSystem.API.HealthChecks;
using RabbitMQ.Client;
using System;

namespace OtpSystem.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddAppHealthChecks(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddHealthChecks()
            .AddRedis(
                config["Redis:ConnectionString"]!,
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "cache", "infrastructure" }
            )
            .AddRabbitMQ(
                // AddRabbitMQ expects a factory that returns an IConnection (Func<IServiceProvider, IConnection>).
                // Build the connection using the IConfiguration from the IServiceProvider.
                sp =>
                {
                    var cfg = sp.GetRequiredService<IConfiguration>();
                    var connStr = $"amqp://{cfg["RabbitMq:Username"]}:{cfg["RabbitMq:Password"]}@{cfg["RabbitMq:Host"]}/";
                    var factory = new ConnectionFactory { Uri = new Uri(connStr) };
                    return factory.CreateConnectionAsync();
                },
                name: "rabbitmq",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["messaging", "infrastructure"]
            )
            //.AddNpgSql(
            //    config["ConnectionStrings:Default"]!,
            //    name: "postgres",
            //    failureStatus: HealthStatus.Unhealthy,
            //    tags: ["database", "infrastructure"]
            //)
            .AddCheck<OtpServiceHealthCheck>(
                "otp-service",
                failureStatus: HealthStatus.Degraded,
                tags: ["application"]
            ); 

        return services;
    }
}