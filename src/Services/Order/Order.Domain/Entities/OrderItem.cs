using EShop.SharedKernel.Domain;

namespace Order.Domain.Entities;

public class OrderItem : IOwnedEntity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    // EF Core constructor
    private OrderItem() { }

    public static OrderItem Create(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
        };
    }
}
