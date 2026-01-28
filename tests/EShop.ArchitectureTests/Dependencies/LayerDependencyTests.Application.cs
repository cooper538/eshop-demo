namespace EShop.ArchitectureTests.Dependencies;

public partial class LayerDependencyTests
{
    [TestMethod]
    [DataRow("Order", nameof(OrderApplicationAssembly))]
    [DataRow("Products", nameof(ProductsApplicationAssembly))]
    public void Application_ShouldNotDependOn_Infrastructure(
        string serviceName,
        string assemblyFieldName
    )
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn($"{serviceName}.Infrastructure")
            .GetResult();

        AssertNoViolations(
            result,
            $"{serviceName}.Application should not depend on {serviceName}.Infrastructure."
        );
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderApplicationAssembly))]
    [DataRow("Products", nameof(ProductsApplicationAssembly))]
    public void Application_ShouldNotDependOn_Api(string serviceName, string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn($"{serviceName}.API")
            .GetResult();

        AssertNoViolations(
            result,
            $"{serviceName}.Application should not depend on {serviceName}.API."
        );
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderApplicationAssembly))]
    [DataRow("Products", nameof(ProductsApplicationAssembly))]
    public void Application_ShouldNotDependOn_ServiceClients(
        string serviceName,
        string assemblyFieldName
    )
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn(ServiceClientsAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            $"{serviceName}.Application should not depend on EShop.ServiceClients (implementation detail)."
        );
    }
}
