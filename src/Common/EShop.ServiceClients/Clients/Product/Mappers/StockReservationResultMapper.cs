using EShop.Contracts.ServiceClients.Product;
using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Mappers;

[Mapper]
public static partial class StockReservationResultMapper
{
    [MapperIgnoreSource(nameof(GrpcProduct.ReserveStockResponse.HasFailureReason))]
    [MapProperty(
        nameof(GrpcProduct.ReserveStockResponse.ErrorCode),
        nameof(StockReservationResult.ErrorCode)
    )]
    public static partial StockReservationResult ToResult(
        this GrpcProduct.ReserveStockResponse response
    );

    private static EStockReservationErrorCode MapErrorCode(GrpcProduct.ErrorCode errorCode)
    {
        return errorCode switch
        {
            GrpcProduct.ErrorCode.None => EStockReservationErrorCode.None,
            GrpcProduct.ErrorCode.InsufficientStock => EStockReservationErrorCode.InsufficientStock,
            GrpcProduct.ErrorCode.ProductNotFound => EStockReservationErrorCode.ProductNotFound,
            _ => EStockReservationErrorCode.None,
        };
    }
}
