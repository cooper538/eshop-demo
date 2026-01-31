using EShop.Common.Application.Cqrs;

namespace EShop.Products.Application.Queries.GetProductsBatch;

public sealed record GetProductsBatchQuery(IReadOnlyList<Guid> ProductIds)
    : IQuery<GetProductsBatchResult>;
