using EShop.Contracts.ServiceClients.Product;
using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Mappers;

[Mapper]
public static partial class ReleaseStockRequestMapper
{
    public static partial GrpcProduct.ReleaseStockRequest ToGrpc(this ReleaseStockRequest request);
}
