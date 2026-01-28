using EShop.SharedKernel.Domain;

namespace EShop.ArchitectureTests.Conventions;

public partial class NamingConventionTests
{
    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void Entities_ShouldEndWith_Entity(string assemblyFieldName)
    {
        var entityTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .Inherit(typeof(Entity))
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var violatingTypes = entityTypes.Where(t => !t.Name.EndsWith("Entity")).ToList();

        AssertNoViolatingTypes(violatingTypes, "All entities should end with 'Entity'.");
    }

    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void AggregateRoots_ShouldEndWith_Entity(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .Inherit(typeof(AggregateRoot))
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Entity")
            .GetResult();

        AssertNoViolations(result, "All aggregate roots should end with 'Entity'.");
    }
}
