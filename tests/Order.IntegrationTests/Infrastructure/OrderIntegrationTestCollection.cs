using EShop.Common.IntegrationTests.Fixtures;

namespace EShop.Order.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class OrderIntegrationTestCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Order Integration Tests";
}
