namespace EShop.E2E.Tests.Fixtures;

public sealed record ProductDto(Guid Id, string Name, string Description, decimal Price);

public sealed record GetProductsResponse(
    List<ProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
