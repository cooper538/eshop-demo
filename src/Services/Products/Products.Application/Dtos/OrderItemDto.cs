namespace Products.Application.Dtos;

public sealed record OrderItemDto(Guid ProductId, int Quantity);
