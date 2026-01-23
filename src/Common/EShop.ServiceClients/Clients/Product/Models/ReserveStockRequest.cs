using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Models;

public sealed record ReserveStockRequest(Guid OrderId, IReadOnlyList<OrderItemRequest> Items);

public sealed record OrderItemRequest(Guid ProductId, int Quantity);

[Mapper]
public static partial class ReserveStockRequestMapper
{
    public static partial GrpcProduct.ReserveStockRequest ToGrpc(this ReserveStockRequest request);
}
