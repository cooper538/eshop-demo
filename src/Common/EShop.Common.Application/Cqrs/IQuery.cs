using MediatR;

namespace EShop.Common.Application.Cqrs;

// Marker for read-only queries (auto NoTracking)
public interface IQuery<TResult> : IRequest<TResult>;
