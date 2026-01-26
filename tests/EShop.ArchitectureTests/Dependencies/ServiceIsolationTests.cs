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
            .HaveDependencyOn(ServiceLayerDefinitions.Products.Domain)
            .GetResult();

        AssertNoViolations(
            result,
            "Order.Domain should not directly depend on Products.Domain (cross-service isolation)."
        );
    }

    [TestMethod]
    public void ProductsDomain_ShouldNotDependOn_OrderDomain()
    {
        var result = Types
            .InAssembly(ProductsDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ServiceLayerDefinitions.Order.Domain)
            .GetResult();

        AssertNoViolations(
            result,
            "Products.Domain should not directly depend on Order.Domain (cross-service isolation)."
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
            .HaveDependencyOn(ServiceLayerDefinitions.Products.Application)
            .GetResult();

        AssertNoViolations(
            result,
            "Order.Application should not directly depend on Products.Application (cross-service isolation)."
        );
    }

    [TestMethod]
    public void ProductsApplication_ShouldNotDependOn_OrderApplication()
    {
        var result = Types
            .InAssembly(ProductsApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ServiceLayerDefinitions.Order.Application)
            .GetResult();

        AssertNoViolations(
            result,
            "Products.Application should not directly depend on Order.Application (cross-service isolation)."
        );
    }

    #endregion
}
