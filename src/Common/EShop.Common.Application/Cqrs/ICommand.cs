using MediatR;

namespace EShop.Common.Application.Cqrs;

// Marker for state-changing commands (auto TrackAll)
public interface ICommand<TResult> : IRequest<TResult>;

public interface ICommand : IRequest;
