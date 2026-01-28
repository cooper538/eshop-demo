namespace EShop.SharedKernel.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
