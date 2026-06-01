using System.Text.Json;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class NotifyOnErrorHandler(ILogger<NotifyOnErrorHandler> logger) : IConstraintHandlerProvider
{
    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "notifyOnError"))
        {
            return [];
        }

        return [new ScopedHandler(new ConstraintHandler.Consumer(Notify), SignalType.Error, 0)];
    }

    private void Notify(object? value)
    {
        if (value is Exception error)
        {
            logger.LogWarning("[ERROR-NOTIFY] Error during policy-protected operation: {Message}", error.Message);
        }
    }
}
