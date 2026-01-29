using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Common.Application.Data;

public interface IChangeTrackerAccessor
{
    ChangeTracker ChangeTracker { get; }
}
