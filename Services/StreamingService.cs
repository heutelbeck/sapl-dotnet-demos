using System.Runtime.CompilerServices;
using Sapl.Demo.Data;

namespace Sapl.Demo.Services;

public sealed class StreamingService : IStreamingService
{
    public IAsyncEnumerable<Heartbeat> HeartbeatTillDenied(CancellationToken ct = default)
    {
        return GenerateHeartbeats(ct);
    }

    public IAsyncEnumerable<Heartbeat> HeartbeatDropWhileDenied(CancellationToken ct = default)
    {
        return GenerateHeartbeats(ct);
    }

    public async IAsyncEnumerable<object> HeartbeatRecoverable(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var hb in GenerateHeartbeats(ct).ConfigureAwait(false))
        {
            yield return hb;
        }
    }

    private static async IAsyncEnumerable<Heartbeat> GenerateHeartbeats(
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
