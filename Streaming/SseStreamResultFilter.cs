using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sapl.Core.Constraints;
using Sapl.Core.Pep.Streaming;

namespace Sapl.Demo.Streaming;

/// <summary>
/// Renders a controller result whose value is an enforced object stream as server-sent events.
/// The SAPL library returns a generic enforced <see cref="IAsyncEnumerable{T}"/> (data items plus
/// boundary and denial markers); this is where the application translates it to SSE. Other
/// results pass through unchanged.
/// </summary>
public sealed class SseStreamResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: IAsyncEnumerable<object?> stream })
        {
            var token = context.HttpContext.RequestAborted;
            await SseResultAdapter.WriteSseStreamAsync(context.HttpContext, MapFrames(stream, token), token).ConfigureAwait(false);
            return;
        }

        await next().ConfigureAwait(false);
    }

    private static async IAsyncEnumerable<object?> MapFrames(
        IAsyncEnumerable<object?> source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item switch
            {
                TransitionReason.Suspended => new StreamSignalFrame("ACCESS_SUSPENDED", "Stream paused by policy"),
                TransitionReason.Granted => new StreamSignalFrame("ACCESS_GRANTED", "Access granted by policy"),
                AccessDeniedException => new StreamSignalFrame("ACCESS_DENIED", "Stream terminated by policy"),
                _ => item,
            };
        }
    }
}
