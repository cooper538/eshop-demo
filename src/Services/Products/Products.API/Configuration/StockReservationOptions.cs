using Microsoft.Extensions.Options;
using Products.Application.Configuration;

namespace Products.API.Configuration;

public sealed class StockReservationOptions : IStockReservationOptions
{
    private readonly ProductSettings _settings;
    private readonly ExpirationOptions _expiration;

    public StockReservationOptions(IOptions<ProductSettings> options)
    {
        _settings = options.Value;
        _expiration = new ExpirationOptions(_settings.StockReservation.Expiration);
    }

    public TimeSpan DefaultDuration => _settings.StockReservation.DefaultDuration;
    public IExpirationOptions Expiration => _expiration;
}

public sealed class ExpirationOptions : IExpirationOptions
{
    private readonly StockExpirationSettings _settings;

    public ExpirationOptions(StockExpirationSettings settings)
    {
        _settings = settings;
    }

    public TimeSpan CheckInterval => _settings.CheckInterval;
    public int BatchSize => _settings.BatchSize;
}
