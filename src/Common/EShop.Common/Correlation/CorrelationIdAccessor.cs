namespace EShop.Common.Correlation;

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string CorrelationId =>
        CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();
}
