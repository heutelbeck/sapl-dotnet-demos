using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class InjectTimestampHandler(IHttpContextAccessor accessor, ILogger<InjectTimestampHandler> logger) : IConstraintHandlerProvider
{
    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "injectTimestamp"))
        {
            return [];
        }

        return [new ScopedHandler(new ConstraintHandler.Runner(Inject), SignalType.Decision, 0)];
    }

    private void Inject()
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        if (accessor.HttpContext is { } httpContext)
        {
            httpContext.Items["policyTimestamp"] = timestamp;
        }

        logger.LogInformation("[METHOD] Injected policy timestamp: {Timestamp}", timestamp);
    }
}
