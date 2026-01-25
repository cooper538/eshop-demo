using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;
using Products.Domain.Entities;
using Products.Domain.Enums;

namespace Products.Application.Commands.ReserveStock;

public sealed class ReserveStockCommandHandler
    : IRequestHandler<ReserveStockCommand, StockReservationResult>
{
    private readonly IProductDbContext _dbContext;

    public ReserveStockCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockReservationResult> Handle(
        ReserveStockCommand request,
        CancellationToken cancellationToken
    )
    {
        // Idempotency check - return existing reservation if found
        var existingReservation = await _dbContext
            .StockReservations.Where(r => r.OrderId == request.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingReservation != null)
        {
            return existingReservation.Status == EReservationStatus.Active
                ? StockReservationResult.Succeeded()
                : StockReservationResult.AlreadyProcessed(existingReservation.Status.ToString());
        }

        // Get all products for reservation
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _dbContext
            .Products.Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        // Check if all products exist
        if (products.Count != productIds.Count)
        {
            var foundIds = products.Select(p => p.Id).ToHashSet();
            var missingIds = productIds.Where(id => !foundIds.Contains(id)).ToList();
            return StockReservationResult.Failed(
                $"Products not found: {string.Join(", ", missingIds)}"
            );
        }

        // Try to reserve stock for each item (all-or-nothing)
        var reservations = new List<StockReservationEntity>();
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            if (!product.ReserveStock(item.Quantity))
            {
                return StockReservationResult.Failed(
                    $"Insufficient stock for product {product.Id} ({product.Name}). "
                        + $"Requested: {item.Quantity}, Available: {product.StockQuantity}"
                );
            }

            var reservation = StockReservationEntity.Create(
                request.OrderId,
                product.Id,
                item.Quantity
            );
            reservations.Add(reservation);
        }

        // All reservations successful - persist (domain events dispatched automatically)
        _dbContext.StockReservations.AddRange(reservations);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return StockReservationResult.Succeeded();
    }
}
