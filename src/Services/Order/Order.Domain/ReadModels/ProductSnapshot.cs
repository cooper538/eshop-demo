namespace EShop.Order.Domain.ReadModels;

public class ProductSnapshot
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public DateTime LastUpdated { get; private set; }

    // EF Core constructor
    private ProductSnapshot() { }

    public static ProductSnapshot Create(
        Guid productId,
        string name,
        decimal price,
        DateTime lastUpdated
    )
    {
        return new ProductSnapshot
        {
            ProductId = productId,
            Name = name,
            Price = price,
            LastUpdated = lastUpdated,
        };
    }

    public void Update(string name, decimal price, DateTime lastUpdated)
    {
        Name = name;
        Price = price;
        LastUpdated = lastUpdated;
    }
}
