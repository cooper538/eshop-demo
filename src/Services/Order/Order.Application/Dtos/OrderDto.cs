using Order.Domain.Entities;

namespace Order.Application.Dtos;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OrderItemDto> Items
)
{
    public static OrderDto FromEntity(OrderEntity entity)
    {
        return new OrderDto(
            entity.Id,
            entity.CustomerId,
            entity.CustomerEmail,
            entity.Status.ToString(),
            entity.TotalAmount,
            entity.RejectionReason,
            entity.CreatedAt,
            entity.UpdatedAt,
            MapItems(entity.Items)
        );
    }

    private static List<OrderItemDto> MapItems(IReadOnlyList<OrderItemEntity> items)
    {
        return items
            .Select(i => new OrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal
            ))
            .ToList();
    }
}
