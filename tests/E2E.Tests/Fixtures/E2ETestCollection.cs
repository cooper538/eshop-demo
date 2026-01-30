namespace EShop.E2E.Tests.Fixtures;

[CollectionDefinition(Name)]
public sealed class E2ETestCollection : ICollectionFixture<E2ETestFixture>
{
    public const string Name = "E2E Tests";
}
