using MediatR;

namespace EShop.Common.Cqrs;

// Marker for state-changing commands (auto TrackAll)
public interface ICommand<TResult> : IRequest<TResult>;

public interface ICommand : IRequest;
