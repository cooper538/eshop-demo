namespace EShop.Contracts.Dtos.Order;

public sealed record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)
{
    public decimal TotalPrice => Quantity * UnitPrice;
}
