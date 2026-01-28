using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Common.Data;

public interface IChangeTrackerAccessor
{
    ChangeTracker ChangeTracker { get; }
}
