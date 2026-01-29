using EShop.Common.Application.Cqrs;
using Products.Application.Dtos;

namespace Products.Application.Commands.ReserveStock;

public sealed record ReserveStockCommand(Guid OrderId, IReadOnlyList<OrderItemDto> Items)
    : ICommand<StockReservationResult>;
