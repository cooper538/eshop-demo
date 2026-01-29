namespace EShop.Common.Application.Correlation;

public sealed class CorrelationContext
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    public string CorrelationId { get; }

    private CorrelationContext(string correlationId)
    {
        CorrelationId = correlationId;
    }

    public static CorrelationContext? Current
    {
        get => CurrentContext.Value;
        private set => CurrentContext.Value = value;
    }

    public static IDisposable CreateScope(string correlationId)
    {
        var previous = Current;
        Current = new CorrelationContext(correlationId);
        return new CorrelationScope(previous);
    }

    private sealed class CorrelationScope : IDisposable
    {
        private readonly CorrelationContext? _previous;
        private bool _disposed;

        public CorrelationScope(CorrelationContext? previous) => _previous = previous;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Current = _previous;
            _disposed = true;
        }
    }
}
