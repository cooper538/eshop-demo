namespace EShop.ServiceClients.Exceptions;

public sealed class ServiceClientException : Exception
{
    public EServiceClientErrorCode ErrorCode { get; }

    public ServiceClientException(
        string message,
        Exception? innerException,
        EServiceClientErrorCode errorCode
    )
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
