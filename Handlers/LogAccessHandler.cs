using System.Text.Json;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class LogAccessHandler(ILogger<LogAccessHandler> logger) : IConstraintHandlerProvider
{
    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "logAccess"))
        {
            return [];
        }

        var message = IConstraintHandlerProvider.StringField(constraint, "message") ?? "Access logged";
        return [new ScopedHandler(new ConstraintHandler.Runner(() => logger.LogInformation("[POLICY] {Message}", message)), SignalType.Decision, 0)];
    }
}
