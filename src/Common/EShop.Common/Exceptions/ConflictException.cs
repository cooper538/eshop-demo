namespace EShop.Common.Exceptions;

public sealed class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message) { }
}
