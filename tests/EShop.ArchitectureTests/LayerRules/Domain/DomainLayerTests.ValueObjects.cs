using EShop.SharedKernel.Domain;

namespace EShop.ArchitectureTests.LayerRules.Domain;

public partial class DomainLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void ValueObjects_ShouldInheritFrom_ValueObject(string assemblyFieldName)
    {
        var valueObjectTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .ResideInNamespaceContaining(".ValueObjects")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var violatingTypes = valueObjectTypes
            .Where(t => !typeof(ValueObject).IsAssignableFrom(t))
            .ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All types in ValueObjects namespace should inherit from ValueObject."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void ValueObjects_ShouldBeSealed(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .Inherit(typeof(ValueObject))
            .Should()
            .BeSealed()
            .GetResult();

        AssertNoViolations(result, "All value objects should be sealed.");
    }

    [TestMethod]
    [DataRow(nameof(OrderDomainAssembly))]
    [DataRow(nameof(ProductsDomainAssembly))]
    public void ValueObjects_ShouldBeIn_DomainLayer(string assemblyFieldName)
    {
        var valueObjectTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .Inherit(typeof(ValueObject))
            .GetTypes();

        AssertAllTypes(
            valueObjectTypes,
            t => t.Namespace?.Contains(".Domain") ?? false,
            t => $"Value object {t.FullName} should be in Domain layer."
        );
    }
}
