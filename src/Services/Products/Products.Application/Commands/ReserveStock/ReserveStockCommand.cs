using EShop.Common.Application.Cqrs;
using EShop.Products.Application.Dtos;

namespace EShop.Products.Application.Commands.ReserveStock;

public sealed record ReserveStockCommand(Guid OrderId, IReadOnlyList<OrderItemDto> Items)
    : ICommand<StockReservationResult>;
