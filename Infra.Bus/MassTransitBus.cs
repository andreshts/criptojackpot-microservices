using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Commands;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using CoreEvent = CryptoJackpot.Domain.Core.Events.Event;

namespace CryptoJackpot.Infra.Bus;

/// <summary>
/// MassTransit implementation of IEventBus.
/// Uses ITopicProducer for Kafka when available, falls back to IPublishEndpoint for in-memory.
/// </summary>
public class MassTransitBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitBus(IServiceProvider serviceProvider, IPublishEndpoint publishEndpoint)
    {
        _serviceProvider = serviceProvider;
        _publishEndpoint = publishEndpoint;
    }

    public Task SendCommand<T>(T command) where T : Command
    {
        return _publishEndpoint.Publish(command);
    }

    public async Task Publish<T>(T @event) where T : CoreEvent
    {
        // Try to get a Kafka producer for this event type
        var kafkaProducer = _serviceProvider.GetService<ITopicProducer<T>>();

        if (kafkaProducer != null)
        {
            // If a Kafka producer is registered, use it
            await kafkaProducer.Produce(@event);
        }
        else
        {
            // Fallback to in-memory bus
            await _publishEndpoint.Publish(@event);
        }
    }
}