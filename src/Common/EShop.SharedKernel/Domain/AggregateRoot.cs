namespace EShop.SharedKernel.Domain;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Value is automatically generated and managed by the database.
    /// </summary>
    public byte[] Version { get; protected set; } = null!;
}
