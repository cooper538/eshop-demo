using EShop.Common.Grpc;
using EShop.Contracts.ServiceClients.Product;
using EShop.ServiceClients.Clients.Product;
using EShop.ServiceClients.Configuration;
using EShop.ServiceClients.Infrastructure.Grpc;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
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

        RegisterGrpcClients(services, options, environment);

        return services;
    }

    private static void RegisterGrpcClients(
        IServiceCollection services,
        ServiceClientOptions options,
        IHostEnvironment environment
    )
    {
        var retryOptions = options.Resilience.Retry;
        var serviceConfig = CreateServiceConfig(retryOptions);

        services.AddTransient<LoggingInterceptor>();
        services.AddTransient<CorrelationIdClientInterceptor>();

        services
            .AddGrpcClient<EShop.Grpc.Product.ProductService.ProductServiceClient>(o =>
            {
                o.Address = new Uri(options.ProductService.Url);
            })
            .ConfigureChannel(o =>
            {
                o.ServiceConfig = serviceConfig;
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
            .AddInterceptor<CorrelationIdClientInterceptor>()
            .AddInterceptor<LoggingInterceptor>();

        services.AddScoped<IProductServiceClient, GrpcProductServiceClient>();
    }

    private static ServiceConfig CreateServiceConfig(RetryOptions retryOptions)
    {
        // gRPC built-in retry with exponential backoff and automatic jitter
        var retryPolicy = new RetryPolicy
        {
            MaxAttempts = retryOptions.MaxRetryCount + 1, // includes initial attempt
            InitialBackoff = TimeSpan.FromMilliseconds(retryOptions.BaseDelayMs),
            MaxBackoff = TimeSpan.FromMilliseconds(retryOptions.MaxBackoffMs),
            BackoffMultiplier = retryOptions.BackoffMultiplier,
            RetryableStatusCodes =
            {
                StatusCode.Unavailable,
                StatusCode.DeadlineExceeded,
                StatusCode.Aborted,
            },
        };

        return new ServiceConfig
        {
            MethodConfigs =
            {
                new MethodConfig { Names = { MethodName.Default }, RetryPolicy = retryPolicy },
            },
        };
    }
}
