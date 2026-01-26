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

        var violatingTypes = entityTypes.Where(t => !typeof(Entity).IsAssignableFrom(t)).ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All entities should inherit from Entity or AggregateRoot."
        );
    }
}
