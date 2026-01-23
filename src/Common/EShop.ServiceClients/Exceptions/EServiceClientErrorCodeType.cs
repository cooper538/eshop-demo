namespace EShop.ServiceClients.Exceptions;

public enum EServiceClientErrorCodeType
{
    Unknown = 0,
    NotFound = 1,
    ValidationError = 2,
    ServiceUnavailable = 3,
    Timeout = 4,
    Unauthorized = 5,
}
