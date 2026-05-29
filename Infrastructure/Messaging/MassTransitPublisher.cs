using MassTransit;
using OtpSystem.Application.Interfaces;

namespace OtpSystem.Infrastructure.Messaging;

public class MassTransitPublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => await _publishEndpoint.Publish(message, ct);
}