using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EShop.Common.Data;

// EF convention: OrderEntity -> Order
public class RemoveEntitySuffixConvention : IModelFinalizingConvention
{
    private const string EntitySuffix = "Entity";

    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context
    )
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var typeName = entityType.ClrType.Name;
            if (typeName.EndsWith(EntitySuffix, StringComparison.Ordinal))
            {
                var newTableName = typeName[..^EntitySuffix.Length];
                entityType.Builder.ToTable(newTableName);
            }
        }
    }
}
