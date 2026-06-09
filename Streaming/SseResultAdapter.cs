using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Sapl.Core.Authorization;

namespace Sapl.Demo.Streaming;

/// <summary>
/// Writes an item stream to the response as server-sent events. This is the demo's rendering of
/// the generic enforced stream the SAPL library returns; the library itself stays SSE-free.
/// </summary>
public static class SseResultAdapter
{
    public static async Task WriteSseStreamAsync<T>(
        HttpContext httpContext,
        IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        httpContext.Response.ContentType = "text/event-stream";
        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers.Connection = "keep-alive";
        httpContext.Response.StatusCode = StatusCodes.Status200OK;

        await httpContext.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var json = JsonSerializer.Serialize(item, SerializerDefaults.Options);
            await httpContext.Response.WriteAsync($"data: {json}\n\n", cancellationToken).ConfigureAwait(false);
            await httpContext.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
