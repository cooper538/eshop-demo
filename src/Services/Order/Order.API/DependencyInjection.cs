using EShop.Common.Api.Extensions;
using EShop.Order.API.Configuration;
using EShop.ServiceClients.Extensions;

namespace EShop.Order.API;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPresentation(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<OrderSettings>()
            .BindConfiguration(OrderSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddServiceClients(builder.Configuration, builder.Environment);

        builder.Services.AddApiDefaults();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        app.UseOpenApiWithSwagger("/api/orders", "Orders API");
        app.MapControllers();
        return app;
    }
}
