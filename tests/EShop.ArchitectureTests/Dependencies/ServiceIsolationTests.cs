namespace EShop.ArchitectureTests.Dependencies;

/// <summary>
/// Tests for cross-service isolation.
/// Services should not directly depend on each other's internal layers.
/// </summary>
[TestClass]
public class ServiceIsolationTests : ArchitectureTestBase
{
    #region Domain Isolation

    [TestMethod]
    public void OrderDomain_ShouldNotDependOn_ProductsDomain()
    {
        var result = Types
            .InAssembly(OrderDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ProductsDomainAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "EShop.Order.Domain should not directly depend on EShop.Products.Domain (cross-service isolation)."
        );
    }

    [TestMethod]
    public void ProductsDomain_ShouldNotDependOn_OrderDomain()
    {
        var result = Types
            .InAssembly(ProductsDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(OrderDomainAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "EShop.Products.Domain should not directly depend on EShop.Order.Domain (cross-service isolation)."
        );
    }

    #endregion

    #region Application Isolation

    [TestMethod]
    public void OrderApplication_ShouldNotDependOn_ProductsApplication()
    {
        var result = Types
            .InAssembly(OrderApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ProductsApplicationAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "EShop.Order.Application should not directly depend on EShop.Products.Application (cross-service isolation)."
        );
    }

    [TestMethod]
    public void ProductsApplication_ShouldNotDependOn_OrderApplication()
    {
        var result = Types
            .InAssembly(ProductsApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(OrderApplicationAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "EShop.Products.Application should not directly depend on EShop.Order.Application (cross-service isolation)."
        );
    }

    #endregion
}
