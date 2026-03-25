using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;

namespace DotNetTask.Application.Abstractions.Messaging;

/// <summary>
/// Abstract base class for all handlers (commands and queries) in the CQRS pattern.
/// Provides shared access to the <see cref="IUnitOfWork"/> for repository operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HandlerBase"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work instance for accessing repositories.</param>
public abstract class HandlerBase(IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Gets the unit of work for accessing repositories.
    /// </summary>
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;
}
