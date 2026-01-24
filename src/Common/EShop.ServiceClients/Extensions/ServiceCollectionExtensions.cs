using EShop.ServiceClients.Clients.Product;
using EShop.ServiceClients.Configuration;
using EShop.ServiceClients.Infrastructure.Grpc;
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
        services.AddTransient<LoggingInterceptor>();
        services.AddTransient<ResilienceInterceptor>();

        services
            .AddGrpcClient<EShop.Grpc.Product.ProductService.ProductServiceClient>(o =>
            {
                o.Address = new Uri(options.ProductService.Url);
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
}
