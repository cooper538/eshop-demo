using Microsoft.Extensions.Logging;

namespace EShop.Common.UnitTests.Fixtures;

public abstract class TestableLogger<T> : ILogger<T>
{
    public abstract IDisposable? BeginScope<TState>(TState state)
        where TState : notnull;

    public abstract bool IsEnabled(LogLevel logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Log(logLevel, eventId, state?.ToString(), exception);
    }

    public abstract void Log(
        LogLevel logLevel,
        EventId eventId,
        string? message,
        Exception? exception
    );
}
