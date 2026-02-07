using Microsoft.Extensions.Configuration;

namespace EShop.AppHost;

public static class ConfigurationForwardingExtensions
{
    public static IResourceBuilder<T> ForwardConfigurationSection<T>(
        this IResourceBuilder<T> resourceBuilder,
        IConfiguration configuration,
        string sectionPath
    )
        where T : IResourceWithEnvironment
    {
        var section = configuration.GetSection(sectionPath);

        foreach (var child in section.GetChildren())
        {
            if (!string.IsNullOrEmpty(child.Value))
            {
                var envVarName = $"{sectionPath}:{child.Key}".Replace(":", "__");
                resourceBuilder.WithEnvironment(envVarName, child.Value);
            }
        }

        return resourceBuilder;
    }
}
