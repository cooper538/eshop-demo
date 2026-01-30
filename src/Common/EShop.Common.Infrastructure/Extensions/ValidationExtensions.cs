using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Common.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddFluentValidation(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}
