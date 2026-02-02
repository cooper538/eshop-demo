namespace EShop.E2E.Tests.Fixtures;

[CollectionDefinition(Name)]
public sealed class E2EAuthTestCollection : ICollectionFixture<E2EAuthTestFixture>
{
    public const string Name = "E2E Auth Tests";
}
