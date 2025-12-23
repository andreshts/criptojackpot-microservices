using CryptoJackpot.Domain.Core.Commands;
using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.Bus;

/// <summary>
/// Event bus abstraction for publishing events and sending commands.
/// Consumer/subscriber registration is handled via DI container (MassTransit consumers).
/// </summary>
public interface IEventBus
{
    Task SendCommand<T>(T command) where T : Command;

    Task Publish<T>(T @event) where T : Event;
}
