using EShop.Common.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Products.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddApplicationServices(typeof(DependencyInjection).Assembly);
        return services;
    }
}
