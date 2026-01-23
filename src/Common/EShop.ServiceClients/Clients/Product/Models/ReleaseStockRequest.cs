using Riok.Mapperly.Abstractions;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product.Models;

public sealed record ReleaseStockRequest(Guid OrderId);

[Mapper]
public static partial class ReleaseStockRequestMapper
{
    public static partial GrpcProduct.ReleaseStockRequest ToGrpc(this ReleaseStockRequest request);
}
