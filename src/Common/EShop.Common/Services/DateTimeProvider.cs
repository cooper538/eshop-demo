using EShop.SharedKernel.Services;

namespace EShop.Common.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
