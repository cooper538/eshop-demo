using Microsoft.AspNetCore.Mvc;

namespace EShop.ArchitectureTests.LayerRules.Api;

/// <summary>
/// Tests for API layer: Controllers.
/// </summary>
[TestClass]
public class ApiLayerTests : ArchitectureTestBase
{
    [TestMethod]
    public void Controllers_ShouldOnlyExistIn_ApiLayer()
    {
        var nonApiAssemblies = new[]
        {
            OrderDomainAssembly,
            OrderApplicationAssembly,
            OrderInfrastructureAssembly,
            ProductsDomainAssembly,
            ProductsApplicationAssembly,
            ProductsInfrastructureAssembly,
        };

        foreach (var assembly in nonApiAssemblies)
        {
            var controllerTypes = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .GetTypes()
                .ToList();

            AssertNoViolatingTypes(
                controllerTypes,
                $"Controllers should only exist in API layer. Found in {assembly.GetName().Name}."
            );
        }
    }

    [TestMethod]
    public void Controllers_ShouldEndWithController()
    {
        var apiAssemblies = new[] { OrderApiAssembly, ProductsApiAssembly };

        foreach (var assembly in apiAssemblies)
        {
            var result = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .Should()
                .HaveNameEndingWith("Controller")
                .GetResult();

            AssertNoViolations(
                result,
                $"All controllers in {assembly.GetName().Name} should end with 'Controller'."
            );
        }
    }

    [TestMethod]
    public void Controllers_ShouldNotDependOn_Infrastructure()
    {
        var apiAssemblies = new[] { OrderApiAssembly, ProductsApiAssembly };

        foreach (var assembly in apiAssemblies)
        {
            var serviceName = assembly.GetName().Name?.Replace(".API", "") ?? "";

            var result = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .ShouldNot()
                .HaveDependencyOn($"{serviceName}.Infrastructure")
                .GetResult();

            AssertNoViolations(
                result,
                $"Controllers in {assembly.GetName().Name} should not depend on Infrastructure (use MediatR)."
            );
        }
    }

    [TestMethod]
    public void Controllers_ShouldNotDependOn_DbContext()
    {
        var apiAssemblies = new[] { OrderApiAssembly, ProductsApiAssembly };

        foreach (var assembly in apiAssemblies)
        {
            var result = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            AssertNoViolations(
                result,
                "Controllers should not depend on EF Core directly (use MediatR handlers)."
            );
        }
    }

    [TestMethod]
    public void Controllers_ShouldNotDependOn_Repositories()
    {
        var apiAssemblies = new[] { OrderApiAssembly, ProductsApiAssembly };

        foreach (var assembly in apiAssemblies)
        {
            var controllerTypes = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .GetTypes();

            AssertAllTypes(
                controllerTypes,
                t =>
                {
                    var constructorParams = t.GetConstructors()
                        .SelectMany(c => c.GetParameters())
                        .Select(p => p.ParameterType);

                    return !constructorParams.Any(p =>
                        p.Name?.Contains("Repository") == true
                        || p.FullName?.Contains("Repository") == true
                    );
                },
                t =>
                    $"Controller {t.FullName} should not depend on repositories directly. Use MediatR."
            );
        }
    }
}
