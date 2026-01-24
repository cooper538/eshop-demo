using EShop.SharedKernel.Domain;

namespace Products.Domain.Entities;

public class ProductEntity : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; }
    public string Category { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public bool IsLowStock => StockQuantity <= LowStockThreshold;

    // EF Core constructor
    private ProductEntity() { }

    public static ProductEntity Create(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        int lowStockThreshold,
        string category
    )
    {
        return new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            LowStockThreshold = lowStockThreshold,
            Category = category,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public bool ReserveStock(int quantity)
    {
        if (StockQuantity < quantity)
        {
            return false;
        }

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ReleaseStock(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        int lowStockThreshold,
        string category
    )
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        LowStockThreshold = lowStockThreshold;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }
}
