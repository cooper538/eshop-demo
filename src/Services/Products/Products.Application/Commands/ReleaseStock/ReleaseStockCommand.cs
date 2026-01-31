using EShop.Common.Application.Cqrs;
using EShop.Products.Application.Dtos;

namespace EShop.Products.Application.Commands.ReleaseStock;

public sealed record ReleaseStockCommand(Guid OrderId) : ICommand<StockReleaseResult>;
