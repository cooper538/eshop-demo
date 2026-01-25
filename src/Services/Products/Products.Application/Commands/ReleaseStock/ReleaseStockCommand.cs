using EShop.Common.Cqrs;
using Products.Application.Dtos;

namespace Products.Application.Commands.ReleaseStock;

public sealed record ReleaseStockCommand(Guid OrderId) : ICommand<StockReleaseResult>;
