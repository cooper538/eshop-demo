using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EShop.Common.Infrastructure.Configuration;

/// <summary>
/// Extension methods for Azure Key Vault configuration integration.
/// </summary>
public static class KeyVaultExtensions
{
    /// <summary>
    /// Adds Azure Key Vault as a configuration source.
    /// Skips silently if Key Vault URI is not configured (local development).
    /// </summary>
    /// <param name="builder">The host application builder</param>
    /// <returns>The host application builder for chaining</returns>
    /// <remarks>
    /// Configuration keys:
    /// - KeyVault:Uri - Key Vault URI (required for Key Vault to be enabled)
    /// - KeyVault:ManagedIdentityClientId - Optional client ID for user-assigned managed identity
    ///
    /// For local development, uses Azure CLI credentials via DefaultAzureCredential.
    /// In Azure, uses Managed Identity (system or user-assigned).
    /// </remarks>
    public static IHostApplicationBuilder AddKeyVaultConfiguration(
        this IHostApplicationBuilder builder
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        var keyVaultUri = builder.Configuration["KeyVault:Uri"];

        if (string.IsNullOrWhiteSpace(keyVaultUri))
        {
            // Key Vault not configured - skip silently (local development scenario)
            return builder;
        }

        var managedIdentityClientId = builder.Configuration["KeyVault:ManagedIdentityClientId"];

        var credentialOptions = new DefaultAzureCredentialOptions();

        if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
        }

        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential(credentialOptions),
            new EShopSecretManager()
        );

        return builder;
    }
}
