namespace EShop.Common.Application.Correlation;

public interface ICorrelationIdAccessor
{
    string CorrelationId { get; }
}
