using MediatR;

namespace EShop.Common.Cqrs;

// Marker for read-only queries (auto NoTracking)
public interface IQuery<TResult> : IRequest<TResult>;
