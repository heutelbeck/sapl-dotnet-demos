using System.Runtime.CompilerServices;
using Sapl.Demo.Data;

namespace Sapl.Demo.Services;

public sealed class StreamingService : IStreamingService
{
    public async IAsyncEnumerable<Heartbeat> Heartbeats([EnumeratorCancellation] CancellationToken ct = default)
    {
        var sequence = 0;
        while (!ct.IsCancellationRequested)
        {
            yield return new Heartbeat(sequence, DateTime.UtcNow.ToString("o"));
            sequence++;
            await Task.Delay(2000, ct);
        }
    }
}
