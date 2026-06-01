using System.Text.Json;
using Sapl.Core.Authorization;
using Sapl.Core.Pep.Constraints;

namespace Sapl.Demo.Handlers;

public sealed class ClassificationFilterHandler(ILogger<ClassificationFilterHandler> logger) : IConstraintHandlerProvider
{
    private static readonly Dictionary<string, int> Levels = new()
    {
        ["PUBLIC"] = 0,
        ["INTERNAL"] = 1,
        ["CONFIDENTIAL"] = 2,
        ["SECRET"] = 3,
    };

    public IReadOnlyList<ScopedHandler> GetConstraintHandlers(JsonElement constraint, IReadOnlySet<SignalType> supportedSignals)
    {
        if (!IConstraintHandlerProvider.ConstraintIsOfType(constraint, "filterByClassification"))
        {
            return [];
        }

        var output = supportedSignals.FirstOrDefault(signal => signal.Kind == SignalKind.Output);
        if (output is null)
        {
            return [];
        }

        var maxLevel = IConstraintHandlerProvider.StringField(constraint, "maxLevel") ?? "PUBLIC";
        var maxRank = Levels.GetValueOrDefault(maxLevel, 0);
        return [new ScopedHandler(new ConstraintHandler.Mapper(value => Filter(value, maxRank)), output, 0)];
    }

    private object? Filter(object? value, int maxRank)
    {
        if (value is null)
        {
            return null;
        }

        var array = value is JsonElement element ? element : JsonSerializer.SerializeToElement(value, SerializerDefaults.Options);
        if (array.ValueKind != JsonValueKind.Array)
        {
            return value;
        }

        var kept = new List<JsonElement>();
        foreach (var item in array.EnumerateArray())
        {
            var classification = item.ValueKind == JsonValueKind.Object &&
                                 item.TryGetProperty("classification", out var field) && field.ValueKind == JsonValueKind.String
                ? field.GetString()
                : null;
            if (classification is not null && Levels.TryGetValue(classification, out var rank) && rank <= maxRank)
            {
                kept.Add(item.Clone());
            }
            else
            {
                logger.LogInformation("[FILTER] excluded {Classification} element", classification);
            }
        }

        return JsonSerializer.SerializeToElement(kept, SerializerDefaults.Options);
    }
}
