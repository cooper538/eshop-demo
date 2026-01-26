using EShop.Common.Cqrs;

namespace EShop.ArchitectureTests.LayerRules.Application;

public partial class ApplicationLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Commands_ShouldImplement_ICommand(string assemblyFieldName)
    {
        var commandTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotAbstract()
            .GetTypes();

        AssertAllTypes(
            commandTypes,
            t =>
                ImplementsInterface(t, typeof(ICommand))
                || ImplementsInterface(t, typeof(ICommand<>)),
            t => $"Command {t.FullName} should implement ICommand or ICommand<T>."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Commands_ShouldResideIn_CommandsNamespace(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining(".Commands")
            .GetResult();

        AssertNoViolations(result, "All commands should be in Commands namespace.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Commands_ShouldBeSealed(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        AssertNoViolations(result, "All commands should be sealed.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Commands_ShouldBeRecords(string assemblyFieldName)
    {
        var commandTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameEndingWith("Handler")
            .And()
            .DoNotHaveNameEndingWith("Validator")
            .GetTypes();

        AssertAllTypes(
            commandTypes,
            IsRecord,
            t => $"Command {t.FullName} should be a record for immutability."
        );
    }
}
