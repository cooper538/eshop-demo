using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EShop.Common.Data;

/// <summary>
/// Provides access to the EF Core ChangeTracker for configuring tracking behavior.
/// Implement this interface in your DbContext to enable automatic tracking configuration.
/// </summary>
public interface IChangeTrackerAccessor
{
    ChangeTracker ChangeTracker { get; }
}
