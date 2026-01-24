using MediatR;

namespace EShop.Common.Cqrs;

/// <summary>
/// Marker interface for read-only queries.
/// Queries using this interface will automatically have NoTracking behavior applied.
/// </summary>
public interface IQuery<TResult> : IRequest<TResult>;
