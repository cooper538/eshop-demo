using EShop.Common.Api.Http;
using EShop.Common.Application.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Common.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }

    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IServiceCollection AddApiDefaults(this IServiceCollection services)
    {
        services.AddCorrelationId();
        services.AddErrorHandling();
        return services;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }

    public static IApplicationBuilder UseApiDefaults(this IApplicationBuilder app)
    {
        app.UseCorrelationId();
        app.UseErrorHandling();
        return app;
    }

    public static WebApplication UseOpenApiWithSwagger(this WebApplication app, string apiName)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", apiName));
        }

        return app;
    }
}
