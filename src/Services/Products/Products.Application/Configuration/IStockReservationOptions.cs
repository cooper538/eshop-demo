namespace EShop.Products.Application.Configuration;

public interface IStockReservationOptions
{
    TimeSpan DefaultDuration { get; }

    IExpirationOptions Expiration { get; }
}

public interface IExpirationOptions
{
    TimeSpan CheckInterval { get; }

    int BatchSize { get; }
}
