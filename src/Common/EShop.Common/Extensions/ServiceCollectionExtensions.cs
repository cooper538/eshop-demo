using EShop.Common.Behaviors;
using EShop.Common.Correlation;
using EShop.Common.Events;
using EShop.Common.Grpc;
using EShop.Common.Middleware;
using EShop.Common.Services;
using EShop.SharedKernel.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }

    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        return services;
    }

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

    public static IServiceCollection AddCommonBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(QueryTrackingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandTrackingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandTrackingBehaviorUnit<,>));
        // UnitOfWork MUST be LAST - dispatches domain events then saves
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehaviorUnit<,>));
        return services;
    }

    public static IServiceCollection AddGrpcCorrelationInterceptors(
        this IServiceCollection services
    )
    {
        services.AddSingleton<CorrelationIdClientInterceptor>();
        services.AddSingleton<CorrelationIdServerInterceptor>();
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
}
