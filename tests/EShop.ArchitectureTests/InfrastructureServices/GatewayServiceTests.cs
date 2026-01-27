namespace EShop.ArchitectureTests.InfrastructureServices;

/// <summary>
/// Architecture tests for API Gateway.
/// Gateway is an infrastructure service - it should only handle routing, rate limiting, and caching.
/// It must NOT contain any business logic or depend on service Domain/Application layers.
/// </summary>
[TestClass]
public class GatewayServiceTests : ArchitectureTestBase
{
    [TestMethod]
    public void Gateway_ShouldNotDependOn_AnyDomainLayer()
    {
        var result = Types
            .InAssembly(GatewayApiAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(OrderDomainAssembly.Name(), ProductsDomainAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "Gateway should not depend on any Domain layer (no business logic)."
        );
    }

    [TestMethod]
    public void Gateway_ShouldNotDependOn_AnyApplicationLayer()
    {
        var result = Types
            .InAssembly(GatewayApiAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                OrderApplicationAssembly.Name(),
                ProductsApplicationAssembly.Name()
            )
            .GetResult();

        AssertNoViolations(
            result,
            "Gateway should not depend on any Application layer (no business logic)."
        );
    }

    [TestMethod]
    public void Gateway_ShouldNotDependOn_AnyInfrastructureLayer()
    {
        var result = Types
            .InAssembly(GatewayApiAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                OrderInfrastructureAssembly.Name(),
                ProductsInfrastructureAssembly.Name()
            )
            .GetResult();

        AssertNoViolations(
            result,
            "Gateway should not depend on any Infrastructure layer (no direct DB access)."
        );
    }

    [TestMethod]
    public void Gateway_ShouldNotDependOn_Contracts()
    {
        // Gateway is a pure reverse proxy - it should not need shared DTOs/events
        var result = Types
            .InAssembly(GatewayApiAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContractsAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "Gateway should not depend on EShop.Contracts (pure routing, no shared DTOs)."
        );
    }

    [TestMethod]
    public void Gateway_ShouldNotDependOn_ServiceClients()
    {
        // Gateway routes requests via YARP, not via service clients
        var result = Types
            .InAssembly(GatewayApiAssembly)
            .ShouldNot()
            .HaveDependencyOn(ServiceClientsAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "Gateway should not depend on EShop.ServiceClients (uses YARP for routing)."
        );
    }

    [TestMethod]
    public void Gateway_ShouldNotDependOn_SharedKernel()
    {
        // Gateway doesn't need domain primitives
        var result = Types
            .InAssembly(GatewayApiAssembly)
            .ShouldNot()
            .HaveDependencyOn(SharedKernelAssembly.Name())
            .GetResult();

        AssertNoViolations(
            result,
            "Gateway should not depend on EShop.SharedKernel (no domain logic)."
        );
    }
}
