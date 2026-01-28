namespace EShop.Common.Correlation;

public interface ICorrelationIdAccessor
{
    string CorrelationId { get; }
}
