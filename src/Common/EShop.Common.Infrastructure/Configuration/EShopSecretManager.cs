using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace EShop.Common.Infrastructure.Configuration;

/// <summary>
/// Custom KeyVaultSecretManager that maps Key Vault secret names to configuration keys.
/// Key Vault doesn't allow colons in secret names, so we use double-dash convention.
/// Example: "ConnectionStrings--productdb" becomes "ConnectionStrings:productdb"
/// </summary>
public sealed class EShopSecretManager
    : Azure.Extensions.AspNetCore.Configuration.Secrets.KeyVaultSecretManager
{
    public override string GetKey(KeyVaultSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);
        return secret.Name.Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
