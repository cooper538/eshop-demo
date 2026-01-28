using EShop.SharedKernel.Domain;

namespace EShop.ArchitectureTests.LayerRules.Domain;

public partial class DomainLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void Entities_ShouldInheritFrom_EntityOrAggregateRoot(string assemblyFieldName)
    {
        var entityTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .ResideInNamespaceEndingWith(".Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        // Exclude owned entities - they have local identity within aggregate, not global identity
        var violatingTypes = entityTypes
            .Where(t => !typeof(Entity).IsAssignableFrom(t))
            .Where(t => !typeof(IOwnedEntity).IsAssignableFrom(t))
            .ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All entities should inherit from Entity or AggregateRoot (except IOwnedEntity types)."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void OwnedEntities_ShouldNotInheritFrom_Entity(string assemblyFieldName)
    {
        var ownedEntityTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .ImplementInterface(typeof(IOwnedEntity))
            .GetTypes();

        // Owned entities should NOT inherit from Entity base class
        var violatingTypes = ownedEntityTypes
            .Where(t => typeof(Entity).IsAssignableFrom(t))
            .ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "Owned entities should NOT inherit from Entity base class (they have local identity only)."
        );
    }
}
