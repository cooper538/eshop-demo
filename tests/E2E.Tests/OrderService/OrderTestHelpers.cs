using System.Net.Http.Json;
using System.Text.Json;
using EShop.E2E.Tests.Fixtures;

namespace EShop.E2E.Tests.OrderService;

public static class OrderTestHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<ProductDto> GetFirstAvailableProductAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>(JsonOptions);
        return result?.Items.FirstOrDefault()
            ?? throw new InvalidOperationException("No products available in database");
    }

    public static object CreateOrderRequest(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity,
        Guid? customerId = null,
        string customerEmail = "test@example.com"
    ) =>
        new
        {
            CustomerId = customerId ?? Guid.NewGuid(),
            CustomerEmail = customerEmail,
            Items = new[]
            {
                new
                {
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                },
            },
        };
}
