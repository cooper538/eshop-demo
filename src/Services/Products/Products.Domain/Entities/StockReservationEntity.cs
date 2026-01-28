using EShop.SharedKernel.Domain;
using Products.Domain.Enums;

namespace Products.Domain.Entities;

public class StockReservationEntity : Entity
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public Guid StockId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public EReservationStatus Status { get; private set; }
    public uint RowVersion { get; private set; }

    // EF Core constructor
    private StockReservationEntity() { }

    internal static StockReservationEntity Create(
        Guid orderId,
        Guid productId,
        int quantity,
        StockEntity stock,
        DateTime reservedAt
    )
    {
        return new StockReservationEntity
        {
            Id = Guid.NewGuid(),
            StockId = stock.Id,
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            ReservedAt = reservedAt,
            ExpiresAt = reservedAt.Add(DefaultTtl),
            Status = EReservationStatus.Active,
        };
    }

    public void Release()
    {
        if (Status != EReservationStatus.Active)
        {
            throw new InvalidOperationException(
                $"Cannot release reservation in status {Status}. Only Active reservations can be released."
            );
        }

        Status = EReservationStatus.Released;
    }

    public void Expire()
    {
        if (Status != EReservationStatus.Active)
        {
            throw new InvalidOperationException(
                $"Cannot expire reservation in status {Status}. Only Active reservations can be expired."
            );
        }

        Status = EReservationStatus.Expired;
    }
}
