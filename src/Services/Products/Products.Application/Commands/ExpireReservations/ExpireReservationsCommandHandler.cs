using System.Linq.Expressions;
using EShop.SharedKernel.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.Data;
using Products.Domain.Entities;
using Products.Domain.Enums;

namespace Products.Application.Commands.ExpireReservations;

public sealed partial class ExpireReservationsCommandHandler
    : IRequestHandler<ExpireReservationsCommand>
{
    private readonly IProductDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ExpireReservationsCommandHandler> _logger;

    public ExpireReservationsCommandHandler(
        IProductDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ILogger<ExpireReservationsCommandHandler> logger
    )
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task Handle(ExpireReservationsCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var isExpired = IsExpiredFilter(now);

        var stocks = await _dbContext
            .Stocks.Include(s => s.Reservations.AsQueryable().Where(isExpired))
            .Where(s => s.Reservations.AsQueryable().Any(isExpired))
            .OrderBy(s => s.Id)
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        if (stocks.Count == 0)
        {
            return;
        }

        var totalExpired = 0;

        foreach (var stock in stocks)
        {
            var expired = stock.ExpireStaleReservations(now);

            foreach (var reservation in expired)
            {
                LogReservationExpired(
                    reservation.OrderId,
                    reservation.ProductId,
                    reservation.Quantity
                );
            }

            totalExpired += expired.Count;
        }

        LogFoundExpiredReservations(totalExpired);
        LogProcessingCompleted(totalExpired);
    }

    private static Expression<Func<StockReservationEntity, bool>> IsExpiredFilter(DateTime now) =>
        r => r.Status == EReservationStatus.Active && r.ExpiresAt < now;

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Found {Count} expired reservations to process"
    )]
    private partial void LogFoundExpiredReservations(int count);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Expired reservation for OrderId {OrderId}, ProductId {ProductId}, Quantity {Quantity}"
    )]
    private partial void LogReservationExpired(Guid orderId, Guid productId, int quantity);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully processed {Count} expired reservations"
    )]
    private partial void LogProcessingCompleted(int count);
}
