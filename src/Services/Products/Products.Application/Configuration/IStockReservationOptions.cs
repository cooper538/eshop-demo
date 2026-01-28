namespace Products.Application.Configuration;

/// <summary>
/// Configuration options for stock reservation.
/// Implemented in API layer to maintain clean architecture separation.
/// </summary>
public interface IStockReservationOptions
{
    /// <summary>
    /// Default duration for stock reservations.
    /// </summary>
    TimeSpan DefaultDuration { get; }

    /// <summary>
    /// Expiration job configuration.
    /// </summary>
    IExpirationOptions Expiration { get; }
}

/// <summary>
/// Configuration options for stock reservation expiration job.
/// </summary>
public interface IExpirationOptions
{
    /// <summary>
    /// Interval between expiration checks.
    /// </summary>
    TimeSpan CheckInterval { get; }

    /// <summary>
    /// Maximum number of reservations to process per batch.
    /// </summary>
    int BatchSize { get; }
}
