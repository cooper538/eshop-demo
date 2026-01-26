using EShop.SharedKernel.Events;

namespace EShop.ArchitectureTests.LayerRules.Domain;

public partial class DomainLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void DomainEvents_ShouldImplement_IDomainEvent(string assemblyFieldName)
    {
        var eventTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .HaveNameEndingWith("Event")
            .And()
            .AreNotAbstract()
            .GetTypes();

        var violatingTypes = eventTypes
            .Where(t => !typeof(IDomainEvent).IsAssignableFrom(t))
            .ToList();

        AssertNoViolatingTypes(violatingTypes, "All domain events should implement IDomainEvent.");
    }

    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void DomainEvents_ShouldInheritFrom_DomainEventBase(string assemblyFieldName)
    {
        var eventTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .HaveNameEndingWith("Event")
            .And()
            .AreNotAbstract()
            .GetTypes();

        var violatingTypes = eventTypes
            .Where(t => !typeof(DomainEventBase).IsAssignableFrom(t))
            .ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All domain events should inherit from DomainEventBase."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void DomainEvents_ShouldBeSealed(string assemblyFieldName)
    {
        var eventTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .ResideInNamespaceContaining(".Events")
            .And()
            .HaveNameEndingWith("Event")
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameMatching(".*Base.*")
            .GetTypes();

        var violatingTypes = eventTypes.Where(t => !t.IsSealed).ToList();

        AssertNoViolatingTypes(violatingTypes, "All domain events should be sealed.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void DomainEvents_ShouldNotBeIn_ApplicationLayer(string assemblyFieldName)
    {
        var assembly = GetAssembly(assemblyFieldName);

        var domainEventTypes = Types
            .InAssembly(assembly)
            .That()
            .Inherit(typeof(DomainEventBase))
            .GetTypes()
            .ToList();

        AssertNoViolatingTypes(
            domainEventTypes,
            $"Domain events should be in Domain layer, not Application ({assembly.GetName().Name})."
        );
    }
}
