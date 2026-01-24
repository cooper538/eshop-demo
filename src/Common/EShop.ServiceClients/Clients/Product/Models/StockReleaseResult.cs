using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Models;

public sealed record StockReleaseResult(bool Success, string? FailureReason = null);

[Mapper]
public static partial class StockReleaseResultMapper
{
    [MapperIgnoreSource(nameof(GrpcProduct.ReleaseStockResponse.HasFailureReason))]
    public static partial StockReleaseResult ToResult(
        this GrpcProduct.ReleaseStockResponse response
    );
}
