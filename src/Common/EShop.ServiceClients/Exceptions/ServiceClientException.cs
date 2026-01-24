namespace EShop.ServiceClients.Exceptions;

/// <summary>
/// Unified exception for service client errors, regardless of protocol.
/// </summary>
public sealed class ServiceClientException : Exception
{
    public EServiceClientErrorCodeType ErrorCode { get; }

    public ServiceClientException(
        string message,
        Exception? innerException,
        EServiceClientErrorCodeType errorCode
    )
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
