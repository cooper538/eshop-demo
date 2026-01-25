using EShop.Common.Cqrs;

namespace Products.Application.Commands.ExpireReservations;

/// <summary>
/// Command to expire stale stock reservations and release stock back to inventory.
/// </summary>
public sealed record ExpireReservationsCommand(int BatchSize = 100) : ICommand;
