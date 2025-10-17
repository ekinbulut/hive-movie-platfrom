using Domain.Events;
using Infrastructure.Messaging.Constants;
using Rebus.Bus;

namespace Infrastructure.Messaging.Services;

public class RebusBus : IMessageBus
{
    private readonly IBus _bus;

    public RebusBus(IBus bus) => _bus = bus;

    public Task Publish<T>(T message) where T : class, IMessage
        => _bus.Publish(message, new Dictionary<string, string>(){
            { RebusHeaders.CausationId,   message.CausationId ?? string.Empty }
        });

    public Task Send<T>(T message) where T : class, IMessage
        => _bus.Send(message, new Dictionary<string, string>() {
            { RebusHeaders.CausationId,   message.CausationId ?? string.Empty }
        });
}

public interface IMessageBus
{
    Task Publish<T>(T message) where T : class, IMessage;
    Task Send<T>(T message) where T : class, IMessage;
}