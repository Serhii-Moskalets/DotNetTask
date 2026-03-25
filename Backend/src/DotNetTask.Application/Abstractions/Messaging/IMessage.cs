using MediatR;

using TinyResult;

namespace DotNetTask.Application.Abstractions.Messaging;

/// <summary>
/// Represents a command that returns a result with data.
/// </summary>
/// <typeparam name="TResponse">The type of the data returned in the result.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

/// <summary>
/// Represents a command that returns a boolean success/failure result.
/// </summary>
public interface ICommand : IRequest<Result<bool>>;

/// <summary>
/// Represents a query that always returns a result with data.
/// </summary>
/// <typeparam name="TResponse">The type of the data returned in the result.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
