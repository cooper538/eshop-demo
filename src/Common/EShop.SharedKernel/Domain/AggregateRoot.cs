namespace EShop.SharedKernel.Domain;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    public uint Version { get; protected set; }
}
