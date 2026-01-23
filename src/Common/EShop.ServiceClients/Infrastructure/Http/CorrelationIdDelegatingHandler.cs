using EShop.Common.Correlation;

namespace EShop.ServiceClients.Infrastructure.Http;

internal sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public CorrelationIdDelegatingHandler(ICorrelationIdAccessor correlationIdAccessor)
    {
        _correlationIdAccessor = correlationIdAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = _correlationIdAccessor.CorrelationId;
        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add(CorrelationIdConstants.HttpHeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
