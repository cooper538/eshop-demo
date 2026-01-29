using EShop.Common.Api.Extensions;
using EShop.Common.Api.Grpc;
using EShop.Common.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.API.Configuration;
using Products.API.Grpc;
using Products.Application.Configuration;

namespace Products.API;

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
        app.UseOpenApiWithSwagger("Products API");
        app.MapControllers();
        app.MapGrpcService<ProductGrpcService>();
        return app;
    }
}
