using EShop.Contracts.ServiceClients.Product;
using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Mappers;

[Mapper]
public static partial class StockReservationResultMapper
{
    [MapperIgnoreSource(nameof(GrpcProduct.ReserveStockResponse.HasFailureReason))]
    public static partial StockReservationResult ToResult(
        this GrpcProduct.ReserveStockResponse response
    );
}
