using Microsoft.AspNetCore.Mvc;
using Sapl.Core.Attributes;
using Sapl.Core.Subscription;

namespace Sapl.Demo.Controllers;

[ApiController]
[Route("api")]
public sealed class ExportController : ControllerBase
{
    private readonly ILogger<ExportController> _logger;

    public ExportController(ILogger<ExportController> logger)
    {
        _logger = logger;
    }

    [HttpGet("exportData/{pilotId}/{sequenceId}")]
    [PreEnforce(Action = "exportData", Customizer = typeof(ExportCustomizer))]
    public IActionResult GetExportData(string pilotId, string sequenceId)
    {
        _logger.LogInformation("exportData pilot={PilotId} sequence={SequenceId}", pilotId, sequenceId);
        return Ok(new { pilotId, sequenceId, data = "export-payload" });
    }

    [HttpGet("exportData2/{pilotId}/{sequenceId}")]
    [PreEnforce(Action = "exportData", Customizer = typeof(ExportCustomizer))]
    public IActionResult GetExportData2(string pilotId, string sequenceId)
    {
        _logger.LogInformation("exportData2 pilot={PilotId} sequence={SequenceId}", pilotId, sequenceId);
        return Ok(new { pilotId, sequenceId, data = "export-payload" });
    }

    // ISubscriptionCustomizer allows programmatic subscription building beyond what
    // attribute string properties can express. The interceptor resolves the customizer
    // from DI (or creates it via ActivatorUtilities) and calls Customize() before
    // building the subscription. Here it forwards the bearer token as secrets and
    // sets the resource with pilotId/sequenceId at the top level (the policy checks
    // resource.pilotId and resource.sequenceId directly).
    private sealed class ExportCustomizer : ISubscriptionCustomizer
    {
        public void Customize(SubscriptionContext context, SubscriptionBuilder builder)
        {
            if (context.BearerToken is not null)
            {
                builder.WithStaticSecrets(new { jwt = context.BearerToken });
            }

            if (context.MethodArguments is not null)
            {
                context.MethodArguments.TryGetValue("pilotId", out var pilotId);
                context.MethodArguments.TryGetValue("sequenceId", out var sequenceId);
                builder.WithStaticResource(new { pilotId, sequenceId });
            }
        }
    }
}
