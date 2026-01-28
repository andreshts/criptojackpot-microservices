using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Commands;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using CoreEvent = CryptoJackpot.Domain.Core.Events.Event;

namespace CryptoJackpot.Infra.Bus;

/// <summary>
/// Implementation of the <see cref="IEventBus"/> interface using MassTransit.
/// Provides support for message publishing through Kafka or an in-memory endpoint.
/// </summary>
/// <remarks>
/// If a Kafka producer is available, it is used for event publishing. Otherwise, the in-memory
/// publish endpoint serves as a fallback.
/// </remarks>
public class MassTransitBus : IEventBus
{
    /// <summary>
    /// Provides access to the application's service container for resolving dependencies at runtime.
    /// </summary>
    /// <remarks>
    /// This allows retrieval of registered services, such as Kafka topic producers or other dependencies,
    /// facilitating dynamic resolution and enabling flexible implementations.
    /// </remarks>
    private readonly IServiceProvider _serviceProvider;
    /// <summary>
    /// Represents the publish endpoint for sending messages to the message bus.
    /// Used as a fallback mechanism for in-memory message publishing when no external producer
    /// (e.g., Kafka) is available.
    /// </summary>
    /// <remarks>
    /// This endpoint is part of the MassTransit message bus integration and is responsible for
    /// handling the publishing of events and commands within the application.
    /// </remarks>
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// MassTransit-based implementation of the <see cref="IEventBus"/> interface.
    /// Responsible for sending commands and publishing events using a combination of Kafka producers and in-memory mechanisms.
    /// </summary>
    public MassTransitBus(IServiceProvider serviceProvider, IPublishEndpoint publishEndpoint)
    {
        _serviceProvider = serviceProvider;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Sends a command to the respective command handler using the configured messaging infrastructure.
    /// </summary>
    /// <typeparam name="T">The type of the command to be sent, which must inherit from <see cref="Command"/>.</typeparam>
    /// <param name="command">The command instance to be sent.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided command is null.</exception>
    /// <remarks>
    /// The command is published using the MassTransit <see cref="IPublishEndpoint"/> implementation. Ensure that
    /// the command type has a corresponding handler configured in the message bus to process the command.
    /// </remarks>
    public Task SendCommand<T>(T command) where T : Command
    {
        return _publishEndpoint.Publish(command);
    }

    /// <summary>
    /// Publishes an event using either a Kafka producer or a fallback in-memory bus,
    /// depending on the available service configuration.
    /// </summary>
    /// <typeparam name="T">The type of the event being published, which must inherit from <see cref="CoreEvent"/>.</typeparam>
    /// <param name="event">The event instance to be published.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// If a Kafka producer for the specific event type is registered, the event will be
    /// published to Kafka. Otherwise, it will fall back to the in-memory message bus
    /// provided by MassTransit.
    /// </remarks>
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