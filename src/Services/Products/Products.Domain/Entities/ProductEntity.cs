using EShop.SharedKernel.Domain;
using Products.Domain.Events;

namespace Products.Domain.Entities;

public class ProductEntity : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private ProductEntity() { }

    public static ProductEntity Create(
        string name,
        string description,
        decimal price,
        int initialStockQuantity,
        int lowStockThreshold,
        string category,
        DateTime createdAt
    )
    {
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            Category = category,
            CreatedAt = createdAt,
        };

        product.AddDomainEvent(
            new ProductCreatedDomainEvent(product.Id, initialStockQuantity, lowStockThreshold)
            {
                OccurredOn = createdAt,
            }
        );

        return product;
    }

    public void Update(
        string name,
        string description,
        decimal price,
        string category,
        int lowStockThreshold,
        DateTime updatedAt
    )
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        UpdatedAt = updatedAt;
        IncrementVersion();

        AddDomainEvent(
            new ProductUpdatedDomainEvent(Id, lowStockThreshold) { OccurredOn = updatedAt }
        );
    }
}
