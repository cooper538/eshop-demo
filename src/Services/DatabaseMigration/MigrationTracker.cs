namespace EShop.DatabaseMigration;

public sealed class MigrationTracker
{
    private readonly object _lock = new();
    private readonly HashSet<string> _pendingMigrations = [];
    private readonly TaskCompletionSource _allCompletedTcs = new();

    public Task AllCompletedTask => _allCompletedTcs.Task;

    public void Register(string contextName)
    {
        lock (_lock)
        {
            _pendingMigrations.Add(contextName);
        }
    }

    public void MarkCompleted(string contextName)
    {
        lock (_lock)
        {
            _pendingMigrations.Remove(contextName);
            if (_pendingMigrations.Count == 0)
            {
                _allCompletedTcs.TrySetResult();
            }
        }
    }

    public void MarkFailed(string contextName, Exception ex)
    {
        lock (_lock)
        {
            _pendingMigrations.Remove(contextName);
            _allCompletedTcs.TrySetException(ex);
        }
    }
}
