using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sapl.Core.Authorization;
using Sapl.Core.Enforcement;
using Sapl.Core.Interception;
using Sapl.Demo.Data;
using Sapl.Demo.Services;

namespace Sapl.Demo.Controllers;

[ApiController]
[Route("api/streaming")]
public sealed class StreamingController : ControllerBase
{
    private readonly IStreamingService _streamingService;
    private readonly EnforcementEngine _engine;

    public StreamingController(IStreamingService streamingService, EnforcementEngine engine)
    {
        _streamingService = streamingService;
        _engine = engine;
    }

    // Till-denied and drop-while-denied: the controller knows nothing about SAPL.
    // Enforcement is fully handled by the proxy on IStreamingService.

    [HttpGet("heartbeat/till-denied")]
    public async Task HeartbeatTillDenied()
    {
        await WriteSseAsync(_streamingService.HeartbeatTillDenied(HttpContext.RequestAborted));
    }

    [HttpGet("heartbeat/drop-while-denied")]
    public async Task HeartbeatDropWhileDenied()
    {
        await WriteSseAsync(_streamingService.HeartbeatDropWhileDenied(HttpContext.RequestAborted));
    }

    // Recoverable enforcement requires IAsyncEnumerable<object> because the interceptor
    // injects AccessSignal items (Denied/Recovered) into the data stream to notify clients
    // of authorization state changes. A concrete type like IAsyncEnumerable<Heartbeat> would
    // make signal injection impossible — the stream would silently pause/resume instead,
    // leaving the client with no visibility into deny/recover transitions.
    [HttpGet("heartbeat/recoverable")]
    public async Task HeartbeatRecoverable()
    {
        var stream = _streamingService.HeartbeatRecoverable(HttpContext.RequestAborted);

        var withSignals = stream.RecoverWith(
            onDenyItem: () => (object)new { type = "ACCESS_SUSPENDED", message = "Waiting for re-authorization" },
            onRecoverItem: () => (object)new { type = "ACCESS_RESTORED", message = "Authorization restored" });

        await WriteSseAsync(withSignals);
    }

    // --- Manual recoverable: uses EnforcementEngine directly with explicit callbacks.
    //     Full control over subscription building and signal handling.
    [HttpGet("heartbeat/recoverable/manual")]
    public async Task HeartbeatRecoverableManual()
    {
        var sub = AuthorizationSubscription.Create("anonymous", "stream:heartbeat", "heartbeat");
        var ct = HttpContext.RequestAborted;
        var output = System.Threading.Channels.Channel.CreateUnbounded<object>();

        var stream = _engine.EnforceRecoverableIfDenied(
            sub,
            () => GenerateHeartbeats(ct),
            onDeny: _ =>
            {
                output.Writer.TryWrite(new { type = "ACCESS_SUSPENDED", message = "Waiting for re-authorization" });
            },
            onRecover: _ =>
            {
                output.Writer.TryWrite(new { type = "ACCESS_RESTORED", message = "Authorization restored" });
            },
            cancellationToken: ct);

        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in stream.WithCancellation(ct))
                {
                    await output.Writer.WriteAsync(item, ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                output.Writer.TryComplete();
            }
        }, ct);

        await WriteSseAsync(output.Reader.ReadAllAsync(ct));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private async Task WriteSseAsync<T>(IAsyncEnumerable<T> stream)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        await foreach (var item in stream.WithCancellation(HttpContext.RequestAborted))
        {
            var json = JsonSerializer.Serialize(item, JsonOptions);
            await Response.WriteAsync($"data: {json}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    private static async IAsyncEnumerable<object> GenerateHeartbeats(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var seq = 0;
        while (!ct.IsCancellationRequested)
        {
            yield return new Heartbeat(seq, DateTime.UtcNow.ToString("o"));
            seq++;
            await Task.Delay(2000, ct);
        }
    }
}
