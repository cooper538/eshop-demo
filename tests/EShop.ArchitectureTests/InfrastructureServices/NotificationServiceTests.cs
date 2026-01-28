using MassTransit;

namespace EShop.ArchitectureTests.InfrastructureServices;

/// <summary>
/// Architecture tests for Notification Service.
/// Notification is an infrastructure service - it consumes integration events and sends notifications.
/// It must NOT depend on service Domain/Application layers, only on EShop.Contracts for events.
/// </summary>
[TestClass]
public class NotificationServiceTests : ArchitectureTestBase
{
    [TestMethod]
    public void Notification_ShouldNotDependOn_AnyDomainLayer()
    {
        var result = Types
            .InAssembly(NotificationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(OrderDomainAssembly.Name(), ProductsDomainAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "Notification should not depend on any Domain layer (event-driven only)."
        );
    }

    [TestMethod]
    public void Notification_ShouldNotDependOn_AnyApplicationLayer()
    {
        var result = Types
            .InAssembly(NotificationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                OrderApplicationAssembly.Name(),
                ProductsApplicationAssembly.Name()
            )
            .GetResult();

        AssertNoViolations(
            result,
            "Notification should communicate only via EShop.Contracts events."
        );
    }

    [TestMethod]
    public void Notification_ShouldNotDependOn_AnyInfrastructureLayer()
    {
        var result = Types
            .InAssembly(NotificationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                OrderInfrastructureAssembly.Name(),
                ProductsInfrastructureAssembly.Name()
            )
            .GetResult();

        AssertNoViolations(
            result,
            "Notification should not depend on any Infrastructure layer (isolated service)."
        );
    }

    [TestMethod]
    public void Notification_ShouldNotDependOn_ServiceClients()
    {
        // Notification is event-driven, not request-driven
        var result = Types
            .InAssembly(NotificationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ServiceClientsAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "Notification should not use ServiceClients (event-driven architecture)."
        );
    }

    [TestMethod]
    public void NotificationConsumers_ShouldEndWith_Consumer()
    {
        // Use reflection directly - NetArchTest has issues with generic interface detection
        var consumers = NotificationAssembly
            .GetTypes()
            .Where(t => !t.IsAbstract && ImplementsGenericInterface(t, typeof(IConsumer<>)))
            .ToList();

        Assert.IsTrue(consumers.Count > 0, "No consumers found in Notification assembly.");

        foreach (var consumer in consumers)
        {
            Assert.IsTrue(
                consumer.Name.EndsWith("Consumer"),
                $"Consumer '{consumer.Name}' should end with 'Consumer' suffix."
            );
        }
    }

    [TestMethod]
    public void NotificationConsumers_ShouldBeSealed()
    {
        // Use reflection directly - NetArchTest has issues with generic interface detection
        var consumers = NotificationAssembly
            .GetTypes()
            .Where(t =>
                !t.IsAbstract
                && !t.Name.StartsWith("Idempotent") // exclude base class
                && ImplementsGenericInterface(t, typeof(IConsumer<>))
            )
            .ToList();

        Assert.IsTrue(consumers.Count > 0, "No consumers found in Notification assembly.");

        var nonSealedConsumers = consumers.Where(t => !t.IsSealed).ToList();

        AssertNoViolatingTypes(
            nonSealedConsumers,
            "All consumers should be sealed for performance (MassTransit best practice)."
        );
    }
}
