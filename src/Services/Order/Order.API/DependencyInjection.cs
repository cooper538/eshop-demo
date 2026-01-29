using EShop.Common.Api.Extensions;
using EShop.Common.Infrastructure.Extensions;
using EShop.ServiceClients.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.API.Configuration;

namespace Order.API;

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
        app.UseOpenApiWithSwagger("Orders API");
        app.MapControllers();
        return app;
    }
}
