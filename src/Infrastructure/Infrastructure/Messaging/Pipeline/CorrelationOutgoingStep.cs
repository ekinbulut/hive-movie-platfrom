using Infrastructure.Messaging.Constants;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Infrastructure.Messaging.Pipeline;

[StepDocumentation("Ensures correlation/causation headers are present on outgoing messages.")]
public class CorrelationOutgoingStep : IOutgoingStep
{
    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        var message = context.Load<Message>();
        if (message == null)
        {
            await next();
            return;
        }

        var headers = message.Headers;

        if (!headers.ContainsKey(RebusHeaders.CorrelationId))
        {
            headers[RebusHeaders.CorrelationId] = Guid.CreateVersion7().ToString("N");
        }

        var current = MessageContext.Current;
        if (current?.Headers.TryGetValue(RebusHeaders.CorrelationId, out var parentCorr) == true)
        {
            headers[RebusHeaders.CausationId] = parentCorr;
        }

        await next();
    }
}