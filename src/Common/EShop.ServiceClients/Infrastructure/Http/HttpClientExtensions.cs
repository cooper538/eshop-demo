using System.Net;
using System.Net.Http.Json;
using EShop.ServiceClients.Exceptions;

namespace EShop.ServiceClients.Infrastructure.Http;

public static class HttpClientExtensions
{
    public static async Task<TResponse> PostAndReadAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ServiceClientException(
                $"HTTP {(int)response.StatusCode} from {endpoint}: {errorBody}",
                innerException: null,
                response.StatusCode.ToErrorCode()
            );
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
            ?? throw new ServiceClientException(
                $"Empty response from {endpoint}",
                innerException: null,
                EServiceClientErrorCodeType.Unknown
            );
    }

    public static EServiceClientErrorCodeType ToErrorCode(this HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.NotFound => EServiceClientErrorCodeType.NotFound,
            HttpStatusCode.BadRequest => EServiceClientErrorCodeType.ValidationError,
            HttpStatusCode.ServiceUnavailable => EServiceClientErrorCodeType.ServiceUnavailable,
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout =>
                EServiceClientErrorCodeType.Timeout,
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                EServiceClientErrorCodeType.Unauthorized,
            _ => EServiceClientErrorCodeType.Unknown,
        };
}
