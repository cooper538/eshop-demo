using EShop.Contracts.ServiceClients.Product;
using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Mappers;

[Mapper]
public static partial class ReserveStockRequestMapper
{
    public static partial GrpcProduct.ReserveStockRequest ToGrpc(this ReserveStockRequest request);
}
