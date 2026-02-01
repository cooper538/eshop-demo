using System.Net;
using System.Net.Http.Json;
using EShop.E2E.Tests.Fixtures;

namespace EShop.E2E.Tests.Gateway;

public class OutputCacheTests : E2ETestBase
{
    public OutputCacheTests(E2ETestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task GetProducts_AfterCreatingNewProduct_ReturnsCachedCount()
    {
        var initialResponse = await GatewayClient.GetAsync("/api/products");
        var initialResult = await initialResponse.Content.ReadFromJsonAsync<GetProductsResponse>(
            JsonOptions
        );
        var initialCount = initialResult!.TotalCount;

        var newProduct = new
        {
            Name = $"Cache Test Product {Guid.NewGuid()}",
            Description = "Product for cache testing",
            Price = 99.99m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            Category = "Test",
        };
        var createResponse = await GatewayClient.PostAsJsonAsync("/api/products", newProduct);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var cachedResponse = await GatewayClient.GetAsync("/api/products");
        var cachedResult = await cachedResponse.Content.ReadFromJsonAsync<GetProductsResponse>(
            JsonOptions
        );

        cachedResponse.Headers.TryGetValues("Age", out var ageValues).Should().BeTrue();
        int.Parse(ageValues!.First()).Should().BeGreaterOrEqualTo(0);
        cachedResult!.TotalCount.Should().Be(initialCount, "cache should return stale count");
    }
}
