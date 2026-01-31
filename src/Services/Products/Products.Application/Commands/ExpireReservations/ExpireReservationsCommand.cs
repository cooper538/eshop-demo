using EShop.Common.Application.Cqrs;

namespace EShop.Products.Application.Commands.ExpireReservations;

public sealed record ExpireReservationsCommand(int BatchSize = 100) : ICommand;
