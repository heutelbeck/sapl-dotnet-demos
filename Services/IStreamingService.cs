using Sapl.Core.Attributes;
using Sapl.Demo.Data;

namespace Sapl.Demo.Services;

public interface IStreamingService
{
    IAsyncEnumerable<Heartbeat> Heartbeats(CancellationToken ct = default);

    // Service-layer streaming enforcement: the attribute sits on the domain method. The object
    // element type carries the boundary/denial markers in-band so a thin controller can render
    // them. Returned through the SAPL proxy as a generic enforced stream.
    [StreamEnforce(Action = "stream:suspend", Resource = "heartbeat", SignalTransitions = true, PauseRapDuringSuspend = true)]
    IAsyncEnumerable<object?> EnforcedHeartbeats(CancellationToken ct = default);
}
