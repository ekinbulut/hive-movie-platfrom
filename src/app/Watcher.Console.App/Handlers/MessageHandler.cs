using Infrastructure.Messaging.Handlers;
using Watcher.Console.App.Events;

namespace Watcher.Console.App.Handlers;


public class MessageHandler : BaseMessageHandler<FileFoundEvent>
{
    protected override Task OnHandle(FileFoundEvent message, string? causationId)
    {
        return Task.CompletedTask;
    }
}