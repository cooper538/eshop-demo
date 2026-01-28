namespace EShop.Common.Exceptions;

public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message)
        : base(message) { }

    public static NotFoundException For<T>(Guid id) =>
        new($"{typeof(T).Name} with ID '{id}' was not found.");
}
