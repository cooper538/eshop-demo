using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace Microsoft.Extensions.Hosting;

public static partial class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddYamlConfiguration(
        this IHostApplicationBuilder builder,
        string serviceName
    )
    {
        var env = builder.Environment.EnvironmentName;

        builder
            .Configuration.AddYamlFile(
                $"{serviceName}.settings.yaml",
                optional: false,
                reloadOnChange: true
            )
            .AddYamlFile(
                $"{serviceName}.settings.{env}.yaml",
                optional: true,
                reloadOnChange: true
            );

        return builder;
    }
}
