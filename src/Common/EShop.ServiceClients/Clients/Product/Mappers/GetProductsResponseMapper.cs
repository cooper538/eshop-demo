using System.Globalization;
using EShop.Contracts.ServiceClients.Product;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Mappers;

public static class GetProductsResponseMapper
{
    public static GetProductsResult ToResult(this GrpcProduct.GetProductsResponse response)
    {
        var products = response.Products.Select(MapProductInfo).ToList();
        return new GetProductsResult(products);
    }

    private static ProductInfo MapProductInfo(GrpcProduct.ProductInfo info)
    {
        return new ProductInfo(
            Guid.Parse(info.ProductId),
            info.Name,
            info.Description,
            decimal.Parse(info.Price, CultureInfo.InvariantCulture),
            info.StockQuantity
        );
    }
}
