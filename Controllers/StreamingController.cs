using Microsoft.AspNetCore.Mvc;
using Sapl.Core.Attributes;
using Sapl.Demo.Data;
using Sapl.Demo.Services;

namespace Sapl.Demo.Controllers;

[ApiController]
[Route("api/streaming")]
public sealed class StreamingController(IStreamingService streamingService) : ControllerBase
{
    [HttpGet("heartbeat/till-denied")]
    [StreamEnforce(Action = "stream:terminate", Resource = "heartbeat")]
    public IAsyncEnumerable<Heartbeat> HeartbeatTillDenied() =>
        streamingService.Heartbeats(HttpContext.RequestAborted);

    [HttpGet("heartbeat/silent-suspending")]
    [StreamEnforce(Action = "stream:suspend", Resource = "heartbeat")]
    public IAsyncEnumerable<Heartbeat> HeartbeatSilentSuspending() =>
        streamingService.Heartbeats(HttpContext.RequestAborted);

    [HttpGet("heartbeat/observed-suspending")]
    [StreamEnforce(Action = "stream:suspend", Resource = "heartbeat", SignalTransitions = true)]
    public IAsyncEnumerable<Heartbeat> HeartbeatObservedSuspending() =>
        streamingService.Heartbeats(HttpContext.RequestAborted);

    // Service-layer streaming: enforcement is on IStreamingService.EnforcedHeartbeats, not here.
    [HttpGet("/api/services/streaming/heartbeat/observed-suspending")]
    public IAsyncEnumerable<object?> ServiceHeartbeatObservedSuspending() =>
        streamingService.EnforcedHeartbeats(HttpContext.RequestAborted);
}
