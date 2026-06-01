using System.Collections.Concurrent;
using System.Text.Json;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class AuditTrailHandler(ILogger<AuditTrailHandler> logger) : IConstraintHandlerProvider
{
    private readonly ConcurrentBag<object> _auditLog = [];

    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "auditTrail"))
        {
            return [];
        }

        var output = supportedSignals.FirstOrDefault(signal => signal.Kind == SignalKind.Output);
        if (output is null)
        {
            return [];
        }

        var action = IConstraintHandlerProvider.StringField(constraint, "action") ?? "unknown";
        return [new ScopedHandler(new ConstraintHandler.Consumer(value => Record(action, value)), output, 0)];
    }

    public IReadOnlyList<object> GetAuditLog() => [.. _auditLog];

    private void Record(string action, object? value)
    {
        _auditLog.Add(new { timestamp = DateTime.UtcNow.ToString("o"), action, value });
        logger.LogInformation("[AUDIT] {Action}: recorded response", action);
    }
}
