using System.Net;
using EShop.ServiceClients.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace EShop.ServiceClients.Infrastructure.Http;

public static class HttpResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(RetryOptions options) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(
                retryCount: options.MaxRetryCount,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * options.BaseDelayMs)
            );

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        CircuitBreakerOptions options
    ) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.FailuresBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(options.BreakDurationSeconds)
            );
}
