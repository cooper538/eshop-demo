namespace EShop.Contracts.ServiceClients.Product;

public sealed record GetProductsResult(IReadOnlyList<ProductInfo> Products);
