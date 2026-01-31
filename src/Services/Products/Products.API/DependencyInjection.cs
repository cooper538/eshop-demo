using EShop.Common.Api.Extensions;
using EShop.Common.Api.Grpc;
using EShop.Products.API.Configuration;
using EShop.Products.API.Grpc;
using EShop.Products.Application.Configuration;
using FluentValidation;

namespace EShop.Products.API;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPresentation(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<ProductSettings>()
            .BindConfiguration(ProductSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<IStockReservationOptions, StockReservationOptions>();

        builder.Services.AddApiDefaults();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<CorrelationIdServerInterceptor>();
            options.Interceptors.Add<GrpcLoggingInterceptor>();
            options.Interceptors.Add<GrpcValidationInterceptor>();
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        return builder;
    }

    public static WebApplication MapProductsEndpoints(this WebApplication app)
    {
        app.UseOpenApiWithSwagger("/api/products", "Products API");
        app.MapControllers();
        app.MapGrpcService<ProductGrpcService>();
        return app;
    }
}
