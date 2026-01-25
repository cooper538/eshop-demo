using EShop.Common.Cqrs;

namespace Products.Application.Queries.GetProductsBatch;

public sealed record GetProductsBatchQuery(IReadOnlyList<Guid> ProductIds)
    : IQuery<GetProductsBatchResult>;
