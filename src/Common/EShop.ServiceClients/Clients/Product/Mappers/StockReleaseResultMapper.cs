using EShop.Contracts.ServiceClients.Product;
using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Mappers;

[Mapper]
public static partial class StockReleaseResultMapper
{
    [MapperIgnoreSource(nameof(GrpcProduct.ReleaseStockResponse.HasFailureReason))]
    public static partial StockReleaseResult ToResult(
        this GrpcProduct.ReleaseStockResponse response
    );
}
