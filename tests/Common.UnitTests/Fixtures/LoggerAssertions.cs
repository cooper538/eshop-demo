using Microsoft.Extensions.Logging;

namespace EShop.Common.UnitTests.Fixtures;

/// <summary>
/// FluentAssertions-style extensions for TestableLogger verification.
/// </summary>
public static class LoggerAssertions
{
    public static void ShouldHaveLogged<T>(
        this Mock<TestableLogger<T>> logger,
        LogLevel logLevel,
        string? messageContains = null,
        int times = 1
    )
    {
        if (messageContains == null)
        {
            logger.Verify(
                l =>
                    l.Log(
                        logLevel,
                        It.IsAny<EventId>(),
                        It.IsAny<string?>(),
                        It.IsAny<Exception?>()
                    ),
                Times.Exactly(times)
            );
        }
        else
        {
            logger.Verify(
                l =>
                    l.Log(
                        logLevel,
                        It.IsAny<EventId>(),
                        It.Is<string?>(m => m != null && m.Contains(messageContains)),
                        It.IsAny<Exception?>()
                    ),
                Times.Exactly(times)
            );
        }
    }

    public static void ShouldNotHaveLogged<T>(
        this Mock<TestableLogger<T>> logger,
        LogLevel logLevel
    )
    {
        logger.Verify(
            l => l.Log(logLevel, It.IsAny<EventId>(), It.IsAny<string?>(), It.IsAny<Exception?>()),
            Times.Never
        );
    }
}
