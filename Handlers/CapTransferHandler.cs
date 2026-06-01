using System.Text.Json;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class CapTransferHandler(ILogger<CapTransferHandler> logger) : IConstraintHandlerProvider
{
    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "capTransferAmount"))
        {
            return [];
        }

        var maxAmount = constraint.TryGetProperty("maxAmount", out var max) && max.TryGetDouble(out var value) ? value : 5000d;
        return [new ScopedHandler(new ConstraintHandler.Mapper(arguments => Cap(arguments, maxAmount)), SignalType.Input, 0)];
    }

    private object? Cap(object? arguments, double maxAmount)
    {
        if (arguments is IDictionary<string, object?> args &&
            args.TryGetValue("amount", out var raw) && raw is double requested && requested > maxAmount)
        {
            logger.LogInformation("[CAP] transfer amount {Requested} -> {Max}", requested, maxAmount);
            args["amount"] = maxAmount;
        }

        return arguments;
    }
}
