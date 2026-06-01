using System.Text.Json;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class EnrichErrorHandler(ILogger<EnrichErrorHandler> logger) : IConstraintHandlerProvider
{
    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "enrichError"))
        {
            return [];
        }

        var supportUrl = IConstraintHandlerProvider.StringField(constraint, "supportUrl") ?? "https://support.example.com";
        return [new ScopedHandler(new ConstraintHandler.Mapper(value => Enrich(value, supportUrl)), SignalType.Error, 0)];
    }

    private object? Enrich(object? value, string supportUrl)
    {
        if (value is not Exception error)
        {
            return value;
        }

        logger.LogInformation("[ERROR-ENRICH] Enriching error with support URL: {SupportUrl}", supportUrl);
        return new InvalidOperationException($"{error.Message} | Support: {supportUrl}", error);
    }
}
