using System.Net.Http.Headers;
using EShop.ServiceClients.Clients.Product;
using EShop.ServiceClients.Configuration;
using EShop.ServiceClients.Infrastructure.Grpc;
using EShop.ServiceClients.Infrastructure.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.ServiceClients.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceClients(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var options =
            configuration.GetSection(ServiceClientOptions.SectionName).Get<ServiceClientOptions>()
            ?? new ServiceClientOptions();

        services.Configure<ServiceClientOptions>(
            configuration.GetSection(ServiceClientOptions.SectionName)
        );

        if (options.Protocol == EServiceProtocol.Grpc)
        {
            RegisterGrpcClients(services, options, environment);
        }
        else
        {
            RegisterHttpClients(services, options);
        }

        return services;
    }

    private static void RegisterGrpcClients(
        IServiceCollection services,
        ServiceClientOptions options,
        IHostEnvironment environment
    )
    {
        services.AddTransient<LoggingInterceptor>();
        services.AddTransient<ResilienceInterceptor>();

        services
            .AddGrpcClient<EShop.Grpc.Product.ProductService.ProductServiceClient>(o =>
            {
                o.Address = new Uri(options.ProductService.GrpcUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                if (environment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                return handler;
            })
            .AddInterceptor<ResilienceInterceptor>()
            .AddInterceptor<LoggingInterceptor>();

        services.AddScoped<IProductServiceClient, GrpcProductServiceClient>();
    }

    private static void RegisterHttpClients(
        IServiceCollection services,
        ServiceClientOptions options
    )
    {
        services.AddTransient<CorrelationIdDelegatingHandler>();
        services.AddTransient<LoggingDelegatingHandler>();

        services
            .AddHttpClient<IProductServiceClient, HttpProductServiceClient>(client =>
            {
                client.BaseAddress = new Uri(options.ProductService.HttpUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddPolicyHandler(HttpResiliencePolicies.GetRetryPolicy(options.Resilience.Retry))
            .AddPolicyHandler(
                HttpResiliencePolicies.GetCircuitBreakerPolicy(options.Resilience.CircuitBreaker)
            );
    }
}
