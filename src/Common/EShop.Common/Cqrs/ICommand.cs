using MediatR;

namespace EShop.Common.Cqrs;

/// <summary>
/// Marker interface for commands that modify state and return a result.
/// Commands using this interface will automatically have TrackAll behavior applied.
/// </summary>
public interface ICommand<TResult> : IRequest<TResult>;

/// <summary>
/// Marker interface for commands that modify state without returning a result.
/// Commands using this interface will automatically have TrackAll behavior applied.
/// </summary>
public interface ICommand : IRequest;
