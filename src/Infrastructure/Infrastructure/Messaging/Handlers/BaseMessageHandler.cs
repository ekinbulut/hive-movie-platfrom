using System.Diagnostics;
using Infrastructure.Messaging.Constants;
using Infrastructure.Messaging.Contracts;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Infrastructure.Messaging.Handlers;

/// <summary>
/// A reusable base for Rebus handlers that:
/// - Extracts correlation/causation headers
/// - Wraps execution in Activity for tracing
/// - Logs start/finish + duration
/// - Offers an overridable idempotency hook
/// </summary>
public abstract class BaseMessageHandler<TMessage> : IHandleMessages<TMessage>
    where TMessage : class, IMessage
{
    private readonly ILogger _logger;

    protected BaseMessageHandler(ILogger logger)
    {
        _logger = logger;
    }

    // Access to raw Rebus context if needed in derived classes
    protected IMessageContext MessageContext => Rebus.Pipeline.MessageContext.Current 
        ?? throw new InvalidOperationException("No Rebus message context in scope.");

    public async Task Handle(TMessage message)
    {
        var headers = MessageContext.Headers;
        var causationId   = headers.TryGetValue(RebusHeaders.CausationId, out var cs) ? cs : message.CausationId;

        var sourceQueue = MessageContext.Headers.TryGetValue(Headers.ReturnAddress, out var from)
            ? from
            : "unknown";
        
        using var activity = new Activity($"Handle {typeof(TMessage).Name}")
            .AddTag("messaging.system", "rebus")
            .AddTag("messaging.destination", sourceQueue)
            .AddTag("messaging.operation", "process")
            .AddTag("causation_id", causationId ?? string.Empty);

        activity.Start();

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("▶️ Handling {MessageType} CorrelationId={CorrelationId} CausationId={CausationId}",
            typeof(TMessage).Name, causationId);

        try
        {
            // Optional idempotency gate: override to implement store check
            if (await IsDuplicateAsync(message, headers))
            {
                _logger.LogWarning("⏭️ Skipping duplicate {MessageType} CorrelationId={CorrelationId}", typeof(TMessage).Name, causationId);
                return;
            }

            await OnHandle(message, causationId);
            _logger.LogInformation("✅ Handled {MessageType} in {Elapsed} ms", typeof(TMessage).Name, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error while handling {MessageType} CorrelationId={CorrelationId}", typeof(TMessage).Name, causationId);
            throw; // let Rebus retry policies kick in
        }
        finally
        {
            sw.Stop();
            activity.Stop();
        }
    }

    /// <summary>
    /// Override to apply idempotency rules (e.g., check a processed table).
    /// Return true to skip processing as duplicate.
    /// </summary>
    protected virtual Task<bool> IsDuplicateAsync(TMessage message, Dictionary<string, string> headers)
        => Task.FromResult(false);

    /// <summary>
    /// Your actual handling logic lives here.
    /// </summary>
    protected abstract Task OnHandle(TMessage message, string? causationId);
}