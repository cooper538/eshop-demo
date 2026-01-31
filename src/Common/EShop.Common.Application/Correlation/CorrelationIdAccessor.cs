namespace EShop.Common.Application.Correlation;

/// DI-friendly accessor for CorrelationId. Analogous to IHttpContextAccessor.
public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string CorrelationId =>
        CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();
}
