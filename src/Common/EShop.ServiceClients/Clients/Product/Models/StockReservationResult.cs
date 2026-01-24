using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Models;

public sealed record StockReservationResult(bool Success, string? FailureReason = null);

[Mapper]
public static partial class StockReservationResultMapper
{
    [MapperIgnoreSource(nameof(GrpcProduct.ReserveStockResponse.HasFailureReason))]
    public static partial StockReservationResult ToResult(
        this GrpcProduct.ReserveStockResponse response
    );
}
