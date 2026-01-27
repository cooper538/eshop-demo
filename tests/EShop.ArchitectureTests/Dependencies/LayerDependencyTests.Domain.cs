namespace EShop.ArchitectureTests.Dependencies;

public partial class LayerDependencyTests
{
    [TestMethod]
    [DataRow("Order", nameof(OrderDomainAssembly))]
    [DataRow("Products", nameof(ProductsDomainAssembly))]
    public void Domain_ShouldNotDependOn_Application(string serviceName, string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn($"{serviceName}.Application")
            .GetResult();

        AssertNoViolations(
            result,
            $"{serviceName}.Domain should not depend on {serviceName}.Application."
        );
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderDomainAssembly))]
    [DataRow("Products", nameof(ProductsDomainAssembly))]
    public void Domain_ShouldNotDependOn_Infrastructure(
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
            $"{serviceName}.Domain should not depend on {serviceName}.Infrastructure."
        );
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderDomainAssembly))]
    [DataRow("Products", nameof(ProductsDomainAssembly))]
    public void Domain_ShouldNotDependOn_Api(string serviceName, string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn($"{serviceName}.API")
            .GetResult();

        AssertNoViolations(result, $"{serviceName}.Domain should not depend on {serviceName}.API.");
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderDomainAssembly))]
    [DataRow("Products", nameof(ProductsDomainAssembly))]
    public void Domain_ShouldNotDependOn_Common(string serviceName, string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn(CommonAssembly.Name())
            .GetResult();

        AssertNoViolations(result, $"{serviceName}.Domain should not depend on EShop.Common.");
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderDomainAssembly))]
    [DataRow("Products", nameof(ProductsDomainAssembly))]
    public void Domain_ShouldNotDependOn_Contracts(string serviceName, string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn(ContractsAssembly.Name())
            .GetResult();

        AssertNoViolations(result, $"{serviceName}.Domain should not depend on EShop.Contracts.");
    }

    [TestMethod]
    [DataRow("Order", nameof(OrderDomainAssembly))]
    [DataRow("Products", nameof(ProductsDomainAssembly))]
    public void Domain_ShouldNotDependOn_ServiceClients(
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
            $"{serviceName}.Domain should not depend on EShop.ServiceClients."
        );
    }
}
