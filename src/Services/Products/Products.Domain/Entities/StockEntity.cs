using EShop.SharedKernel.Domain;
using Products.Domain.Enums;
using Products.Domain.Events;

namespace Products.Domain.Entities;

public class StockEntity : AggregateRoot
{
    private readonly List<StockReservationEntity> _reservations = [];

    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int LowStockThreshold { get; private set; }

    public IReadOnlyCollection<StockReservationEntity> Reservations => _reservations.AsReadOnly();

    public int AvailableQuantity => CalculateAvailableQuantity();

    public bool IsLowStock => AvailableQuantity <= LowStockThreshold;

    // EF Core constructor
    private StockEntity() { }

    public static StockEntity Create(Guid productId, int initialQuantity, int lowStockThreshold)
    {
        return new StockEntity
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = initialQuantity,
            LowStockThreshold = lowStockThreshold,
        };
    }

    public StockReservationEntity ReserveStock(
        Guid orderId,
        int quantity,
        DateTime reservedAt,
        TimeSpan reservationDuration
    )
    {
        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Requested: {quantity}, Available: {AvailableQuantity}"
            );
        }

        var reservation = StockReservationEntity.Create(
            orderId,
            ProductId,
            quantity,
            this,
            reservedAt,
            reservationDuration
        );
        _reservations.Add(reservation);

        if (IsLowStock)
        {
            AddDomainEvent(
                new LowStockWarningDomainEvent(ProductId, AvailableQuantity, LowStockThreshold)
                {
                    OccurredOn = reservedAt,
                }
            );
        }

        return reservation;
    }

    public void ReleaseReservation(Guid orderId)
    {
        var reservations = _reservations
            .Where(r => r.OrderId == orderId && r.Status == EReservationStatus.Active)
            .ToList();

        foreach (var reservation in reservations)
        {
            reservation.Release();
        }
    }

    public IReadOnlyList<StockReservationEntity> ExpireStaleReservations(DateTime now)
    {
        var expiredReservations = _reservations
            .Where(r => r.Status == EReservationStatus.Active && r.ExpiresAt < now)
            .ToList();

        foreach (var reservation in expiredReservations)
        {
            reservation.Expire();
            AddDomainEvent(
                new StockReservationExpiredDomainEvent(
                    reservation.OrderId,
                    reservation.ProductId,
                    reservation.Quantity
                )
                {
                    OccurredOn = now,
                }
            );
        }

        return expiredReservations;
    }

    public void UpdateLowStockThreshold(int threshold) => LowStockThreshold = threshold;

    private int CalculateAvailableQuantity()
    {
        var reservedQuantity = _reservations
            .Where(r => r.Status == EReservationStatus.Active)
            .Sum(r => r.Quantity);

        return Quantity - reservedQuantity;
    }
}
