using FluentValidation;

namespace EShop.ArchitectureTests.LayerRules.Application;

public partial class ApplicationLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Validators_ShouldInheritFrom_AbstractValidator(string assemblyFieldName)
    {
        var validatorTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Validator")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        AssertAllTypes(
            validatorTypes,
            t => IsSubclassOfGeneric(t, typeof(AbstractValidator<>)),
            t => $"Validator {t.FullName} should inherit from AbstractValidator<T>."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void CommandValidators_ShouldResideIn_CommandsNamespace(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("CommandValidator")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining(".Commands")
            .GetResult();

        AssertNoViolations(result, "All command validators should be in Commands namespace.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void QueryValidators_ShouldResideIn_QueriesNamespace(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("QueryValidator")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining(".Queries")
            .GetResult();

        AssertNoViolations(result, "All query validators should be in Queries namespace.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Validators_ShouldBeSealed(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Validator")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        AssertNoViolations(result, "All validators should be sealed.");
    }
}
