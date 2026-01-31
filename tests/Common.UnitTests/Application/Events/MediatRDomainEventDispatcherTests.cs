using EShop.Common.Application.Events;
using EShop.Common.UnitTests.SharedKernel.TestDoubles;
using EShop.SharedKernel.Events;
using MediatR;

namespace EShop.Common.UnitTests.Application.Events;

public class MediatRDomainEventDispatcherTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly MediatRDomainEventDispatcher _dispatcher;

    public MediatRDomainEventDispatcherTests()
    {
        _publisherMock = new Mock<IPublisher>();
        _dispatcher = new MediatRDomainEventDispatcher(_publisherMock.Object);
    }

    [Fact]
    public async Task Dispatch_WrapsEventInNotification_PublishesViaMediatR()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("Test message");
        object? publishedNotification = null;

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>(
                (notification, _) => publishedNotification = notification
            );

        // Act
        await _dispatcher.DispatchAsync(domainEvent, CancellationToken.None);

        // Assert
        publishedNotification.Should().NotBeNull();
        publishedNotification.Should().BeOfType<DomainEventNotification<TestDomainEvent>>();
        var notification = (DomainEventNotification<TestDomainEvent>)publishedNotification!;
        notification.DomainEvent.Should().Be(domainEvent);
    }

    [Fact]
    public async Task Dispatch_MultipleEvents_PublishesAll()
    {
        // Arrange
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent("Event 1"),
            new TestDomainEvent("Event 2"),
            new TestDomainEvent("Event 3"),
        };

        // Act
        await _dispatcher.DispatchAsync(events, CancellationToken.None);

        // Assert - Publish is called with object parameter, not generic
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
    }

    [Fact]
    public async Task Dispatch_SameEventTypeTwice_UsesCachedConstructor()
    {
        // Arrange
        var event1 = new TestDomainEvent("First");
        var event2 = new TestDomainEvent("Second");
        var publishedNotifications = new List<object>();

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>(
                (notification, _) => publishedNotifications.Add(notification)
            );

        // Act - dispatch same event type twice
        await _dispatcher.DispatchAsync(event1, CancellationToken.None);
        await _dispatcher.DispatchAsync(event2, CancellationToken.None);

        // Assert - both events should be published (cache should work transparently)
        publishedNotifications.Should().HaveCount(2);
        publishedNotifications.Should().AllBeOfType<DomainEventNotification<TestDomainEvent>>();

        var notifications = publishedNotifications
            .Cast<DomainEventNotification<TestDomainEvent>>()
            .ToList();
        notifications[0].DomainEvent.Should().Be(event1);
        notifications[1].DomainEvent.Should().Be(event2);
    }
}
