using Microsoft.EntityFrameworkCore;

namespace EShop.ArchitectureTests.LayerRules.Infrastructure;

/// <summary>
/// Tests for Infrastructure layer: DbContext, Repositories.
/// </summary>
[TestClass]
public class InfrastructureLayerTests : ArchitectureTestBase
{
    [TestMethod]
    public void DbContext_ShouldOnlyExistIn_InfrastructureLayer()
    {
        var nonInfrastructureAssemblies = new[]
        {
            OrderDomainAssembly,
            OrderApplicationAssembly,
            OrderApiAssembly,
            ProductsDomainAssembly,
            ProductsApplicationAssembly,
            ProductsApiAssembly,
        };

        foreach (var assembly in nonInfrastructureAssemblies)
        {
            var dbContextTypes = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(DbContext))
                .GetTypes()
                .ToList();

            AssertNoViolatingTypes(
                dbContextTypes,
                $"DbContext should only exist in Infrastructure layer. Found in {assembly.GetName().Name}."
            );
        }
    }
}
