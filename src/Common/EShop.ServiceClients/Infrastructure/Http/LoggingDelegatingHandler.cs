using Microsoft.Extensions.Logging;

namespace EShop.ServiceClients.Infrastructure.Http;

internal sealed class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var operation = request.RequestUri?.AbsolutePath ?? "unknown";

        _logger.HttpCallStarted(request.Method.Method, operation);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.HttpCallFailed(request.Method.Method, operation, (int)response.StatusCode);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.HttpError(ex, operation);
            throw;
        }
    }
}
