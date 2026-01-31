using EShop.Common.Application.Behaviors;
using EShop.Common.Application.Data;
using EShop.Common.Application.Events;
using EShop.Common.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Common.UnitTests.Application.Behaviors;

public class UnitOfWorkExecutorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<TestableLogger<UnitOfWorkExecutor>> _loggerMock;

    public UnitOfWorkExecutorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _loggerMock = new Mock<TestableLogger<UnitOfWorkExecutor>>();
    }

    [Fact]
    public async Task Execute_WithConcurrencyException_ThrowsConflictException()
    {
        // Arrange
        var executor = new UnitOfWorkExecutor(
            _eventDispatcherMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            changeTrackerAccessor: null
        );

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var act = () => executor.ExecuteAsync(next, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Entity was modified by another user.");
    }

    [Fact]
    public async Task Execute_NullUnitOfWork_LogsWarningAndCompletes()
    {
        // Arrange
        var executor = new UnitOfWorkExecutor(
            _eventDispatcherMock.Object,
            _loggerMock.Object,
            unitOfWork: null,
            changeTrackerAccessor: null
        );

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var result = await executor.ExecuteAsync(next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        _loggerMock.ShouldHaveLogged(LogLevel.Warning, "UnitOfWork is null");
    }

    [Fact]
    public async Task Execute_NoEvents_SavesWithoutDispatch()
    {
        // Arrange
        var executor = new UnitOfWorkExecutor(
            _eventDispatcherMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            changeTrackerAccessor: null
        );

        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act
        var result = await executor.ExecuteAsync(next, CancellationToken.None);

        // Assert
        result.Should().Be("success");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcherMock.Verify(
            d =>
                d.DispatchAsync(
                    It.IsAny<IEnumerable<EShop.SharedKernel.Events.IDomainEvent>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Execute_WhenHandlerThrows_DoesNotSave()
    {
        // Arrange
        var executor = new UnitOfWorkExecutor(
            _eventDispatcherMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            changeTrackerAccessor: null
        );

        RequestHandlerDelegate<string> next = () =>
            throw new InvalidOperationException("Handler failed");

        // Act
        var act = () => executor.ExecuteAsync(next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler failed");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
