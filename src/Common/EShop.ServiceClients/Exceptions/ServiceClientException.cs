namespace EShop.ServiceClients.Exceptions;

/// <summary>
/// Unified exception for service client errors, regardless of protocol.
/// </summary>
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
