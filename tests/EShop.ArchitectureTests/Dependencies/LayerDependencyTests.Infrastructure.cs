namespace EShop.ArchitectureTests.Dependencies;

public partial class LayerDependencyTests
{
    [TestMethod]
    [DataRow("Order", nameof(OrderInfrastructureAssembly))]
    [DataRow("Products", nameof(ProductsInfrastructureAssembly))]
    public void Infrastructure_ShouldNotDependOn_Api(string serviceName, string assemblyFieldName)
    {
        var result = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .ShouldNot()
            .HaveDependencyOn($"{serviceName}.API")
            .GetResult();

        AssertNoViolations(
            result,
            $"{serviceName}.Infrastructure should not depend on {serviceName}.API."
        );
    }
}
