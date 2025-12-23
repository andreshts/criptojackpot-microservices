using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Commands;
using MassTransit;
using CoreEvent = CryptoJackpot.Domain.Core.Events.Event;

namespace CryptoJackpot.Infra.Bus;

/// <summary>
/// MassTransit implementation of IEventBus.
/// Consumers are registered via DI container using MassTransit's AddConsumer.
/// </summary>
public class MassTransitBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task SendCommand<T>(T command) where T : Command
    {
        return publishEndpoint.Publish(command);
    }

    public Task Publish<T>(T @event) where T : CoreEvent
    {
        return publishEndpoint.Publish(@event);
    }
}