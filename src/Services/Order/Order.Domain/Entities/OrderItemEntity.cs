namespace Order.Domain.Entities;

public class OrderItemEntity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    // EF Core constructor
    private OrderItemEntity() { }

    public static OrderItemEntity Create(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice
    )
    {
        return new OrderItemEntity
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
        };
    }
}
