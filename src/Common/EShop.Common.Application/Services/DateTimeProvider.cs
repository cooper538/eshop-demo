using EShop.SharedKernel.Services;

namespace EShop.Common.Application.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
