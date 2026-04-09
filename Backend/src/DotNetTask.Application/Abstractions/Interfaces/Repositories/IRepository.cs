using DotNetTask.Domain.Common;

namespace DotNetTask.Application.Abstractions.Interfaces.Repositories;

/// <summary>
/// Generic repository interface providing basic CRUD operations for any entity type.
/// This interface defines asynchronous methods for retrieving, adding, updating, and deleting entities,
/// as well as checking existence and saving changes to the database.
/// </summary>
/// <typeparam name="TEntity">The type of the entity that the repository will manage. Must be a class.</typeparam>
public interface IRepository
    <TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier, optionally as a no-tracking query.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the entity will be loaded without tracking, which improves performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an entity exists by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>Generic type constraints should be on their own line.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);
}
