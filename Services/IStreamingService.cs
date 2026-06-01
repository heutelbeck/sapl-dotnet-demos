using Sapl.Demo.Data;

namespace Sapl.Demo.Services;

public interface IStreamingService
{
    IAsyncEnumerable<Heartbeat> Heartbeats(CancellationToken ct = default);
}
