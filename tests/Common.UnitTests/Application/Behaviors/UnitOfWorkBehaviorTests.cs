using EShop.Common.Application.Behaviors;
using EShop.Common.Application.Cqrs;
using EShop.Common.Application.Data;
using EShop.Common.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EShop.Common.UnitTests.Application.Behaviors;

public class UnitOfWorkBehaviorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ILogger<UnitOfWorkBehavior<TestCommand, string>> _logger;

    public UnitOfWorkBehaviorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _logger = NullLogger<UnitOfWorkBehavior<TestCommand, string>>.Instance;
    }

    [Fact]
    public async Task Handle_WithConcurrencyException_ThrowsConflictException()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(_logger, _unitOfWorkMock.Object);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var act = () => behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Entity was modified by another user.");
    }

    [Fact]
    public async Task Handle_NullUnitOfWork_Completes()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(_logger, unitOfWork: null);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
    }

    [Fact]
    public async Task Handle_CallsSaveChanges()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(_logger, _unitOfWorkMock.Object);

        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        result.Should().Be("success");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_DoesNotSave()
    {
        // Arrange
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(_logger, _unitOfWorkMock.Object);

        RequestHandlerDelegate<string> next = () =>
            throw new InvalidOperationException("Handler failed");

        // Act
        var act = () => behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler failed");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

public sealed record TestCommand : ICommand<string>;
