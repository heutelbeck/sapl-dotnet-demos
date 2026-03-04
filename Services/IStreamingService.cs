using Sapl.Core.Attributes;
using Sapl.Demo.Data;

namespace Sapl.Demo.Services;

public interface IStreamingService
{
    [EnforceTillDenied(Action = "stream:heartbeat", Resource = "heartbeat")]
    IAsyncEnumerable<Heartbeat> HeartbeatTillDenied(CancellationToken ct = default);

    [EnforceDropWhileDenied(Action = "stream:heartbeat", Resource = "heartbeat")]
    IAsyncEnumerable<Heartbeat> HeartbeatDropWhileDenied(CancellationToken ct = default);

    // Returns IAsyncEnumerable<object> (not Heartbeat) because the recoverable enforcement
    // interceptor injects AccessSignal items into the stream to notify clients of deny/recover
    // transitions. Using a concrete type would bypass signal injection entirely.
    [EnforceRecoverableIfDenied(Action = "stream:heartbeat", Resource = "heartbeat")]
    IAsyncEnumerable<object> HeartbeatRecoverable(CancellationToken ct = default);
}
