using MediatR;

namespace EShop.ArchitectureTests.LayerRules.Application;

public partial class ApplicationLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Handlers_ShouldImplement_IRequestHandlerOrINotificationHandler(
        string assemblyFieldName
    )
    {
        var handlerTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        AssertAllTypes(
            handlerTypes,
            t =>
                ImplementsAnyGenericInterface(
                    t,
                    typeof(IRequestHandler<>),
                    typeof(IRequestHandler<,>),
                    typeof(INotificationHandler<>)
                ),
            t =>
                $"Handler {t.FullName} should implement IRequestHandler<> or INotificationHandler<>."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void CommandHandlers_ShouldResideIn_CommandsNamespace(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("CommandHandler")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining(".Commands")
            .GetResult();

        AssertNoViolations(result, "All command handlers should be in Commands namespace.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void QueryHandlers_ShouldResideIn_QueriesNamespace(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("QueryHandler")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining(".Queries")
            .GetResult();

        AssertNoViolations(result, "All query handlers should be in Queries namespace.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Handlers_ShouldBeSealed(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        AssertNoViolations(result, "All handlers should be sealed.");
    }
}
