using EShop.Products.IntegrationTests.Fixtures;

namespace EShop.Products.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ProductIntegrationTestCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Product Integration Tests";
}
