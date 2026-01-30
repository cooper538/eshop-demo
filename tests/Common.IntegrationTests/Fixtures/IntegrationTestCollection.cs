namespace EShop.Common.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection
    : ICollectionFixture<PostgresContainerFixture>,
        ICollectionFixture<RabbitMqContainerFixture>
{
    public const string Name = "Integration Tests";
}
