namespace EShop.Common.Application.Correlation;

public static class CorrelationIdConstants
{
    public const string HttpHeaderName = "X-Correlation-ID";
    public const string GrpcMetadataKey = "x-correlation-id";
    public const string MassTransitHeaderKey = "X-Correlation-ID";
    public const string LoggingScopeKey = "CorrelationId";
}
