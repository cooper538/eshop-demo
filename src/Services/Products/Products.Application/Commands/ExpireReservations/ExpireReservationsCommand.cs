using EShop.Common.Cqrs;

namespace Products.Application.Commands.ExpireReservations;

public sealed record ExpireReservationsCommand(int BatchSize = 100) : ICommand;
