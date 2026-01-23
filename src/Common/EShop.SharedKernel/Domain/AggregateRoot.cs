namespace EShop.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public uint Version { get; protected set; }
}
