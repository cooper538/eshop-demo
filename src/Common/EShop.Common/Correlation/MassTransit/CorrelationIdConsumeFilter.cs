using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Correlation.MassTransit;

public sealed class CorrelationIdConsumeFilter<T>(ILogger<CorrelationIdConsumeFilter<T>> logger)
    : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId =
            context.Headers.Get<string>(CorrelationIdConstants.MassTransitHeaderKey)
            ?? context.CorrelationId?.ToString()
            ?? Guid.NewGuid().ToString();

        using (CorrelationContext.CreateScope(correlationId))
        using (
            logger.BeginScope(
                new Dictionary<string, object>
                {
                    [CorrelationIdConstants.LoggingScopeKey] = correlationId,
                }
            )
        )
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlationIdConsumeFilter");
}
