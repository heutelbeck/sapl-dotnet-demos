using System.Text.Json;
using System.Text.Json.Nodes;
using Sapl.Core.Authorization;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class RedactFieldsHandler(ILogger<RedactFieldsHandler> logger) : IConstraintHandlerProvider
{
    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "redactFields"))
        {
            return [];
        }

        var output = supportedSignals.FirstOrDefault(signal => signal.Kind == SignalKind.Output);
        if (output is null)
        {
            return [];
        }

        var fields = ReadFields(constraint);
        return [new ScopedHandler(new ConstraintHandler.Mapper(value => Redact(value, fields)), output, 0)];
    }

    private object? Redact(object? value, IReadOnlyList<string> fields)
    {
        if (value is null)
        {
            return null;
        }

        var json = value is JsonElement element ? element.GetRawText() : JsonSerializer.Serialize(value, SerializerDefaults.Options);
        if (JsonNode.Parse(json) is not JsonObject obj)
        {
            return value;
        }

        foreach (var field in fields.Where(obj.ContainsKey))
        {
            obj[field] = "[REDACTED]";
            logger.LogInformation("[REDACT] Redacting field: {Field}", field);
        }

        return JsonDocument.Parse(obj.ToJsonString()).RootElement.Clone();
    }

    private static List<string> ReadFields(JsonElement constraint)
    {
        var fields = new List<string>();
        if (constraint.TryGetProperty("fields", out var array) && array.ValueKind == JsonValueKind.Array)
        {
            fields.AddRange(array.EnumerateArray().Select(field => field.GetString()).OfType<string>());
        }

        return fields;
    }
}
