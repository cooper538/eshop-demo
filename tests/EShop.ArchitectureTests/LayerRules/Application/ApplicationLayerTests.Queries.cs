using EShop.Common.Application.Cqrs;

namespace EShop.ArchitectureTests.LayerRules.Application;

public partial class ApplicationLayerTests
{
    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Queries_ShouldImplement_IQuery(string assemblyFieldName)
    {
        var queryTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreNotAbstract()
            .GetTypes();

        AssertAllTypes(
            queryTypes,
            t => ImplementsGenericInterface(t, typeof(IQuery<>)),
            t => $"Query {t.FullName} should implement IQuery<T>."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Queries_ShouldResideIn_QueriesNamespace(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining(".Queries")
            .GetResult();

        AssertNoViolations(result, "All queries should be in Queries namespace.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Queries_ShouldBeSealed(string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        AssertNoViolations(result, "All queries should be sealed.");
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Queries_ShouldBeRecords(string assemblyFieldName)
    {
        var queryTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameEndingWith("Handler")
            .And()
            .DoNotHaveNameEndingWith("Validator")
            .GetTypes();

        AssertAllTypes(
            queryTypes,
            IsRecord,
            t => $"Query {t.FullName} should be a record for immutability."
        );
    }
}
