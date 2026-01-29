using EShop.Common.Application.Correlation;
using MassTransit;

namespace EShop.Common.Infrastructure.Correlation.MassTransit;

public sealed class CorrelationIdSendFilter<T> : IFilter<SendContext<T>>
    where T : class
{
    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        var correlationId = CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();

        context.Headers.Set(CorrelationIdConstants.MassTransitHeaderKey, correlationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("correlationIdSendFilter");
}
