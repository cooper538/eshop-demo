using EShop.Common.Application.Correlation;
using MassTransit;

namespace EShop.Common.Infrastructure.Correlation.MassTransit;

public sealed class CorrelationIdPublishFilter<T> : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var correlationId = CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();

        context.Headers.Set(CorrelationIdConstants.MassTransitHeaderKey, correlationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlationIdPublishFilter");
}
