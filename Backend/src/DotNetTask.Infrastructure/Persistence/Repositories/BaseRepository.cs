using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Domain.Common;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository providing basic CRUD operations for any entity type.
/// This class implements <see cref="IRepository{TEntity}"/> and serves as a base
/// for concrete repositories that manage specific entity types.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be managed. Must be a class.</typeparam>
public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{TEntity}"/> class
    /// with the specified database context.
    /// </summary>
    /// <param name="context">The database context used for data operations.</param>
    protected BaseRepository(DotNetTaskDbContext context)
    {
        this.Context = context;
        this.DbSet = this.Context.Set<TEntity>();
    }

    /// <summary>
    /// Gets the database context used by this repository.
    /// </summary>
    protected DotNetTaskDbContext Context { get; }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> representing the collection of entities in the database.
    /// </summary>
    protected DbSet<TEntity> DbSet { get; }

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    /// <remarks>
    /// Changes are not automatically saved. Call SaveChangesAsync in service if needed.
    /// </remarks>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await this.DbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier, optionally using a no-tracking query.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns the entity if found; otherwise, <c>null</c>.
    /// </returns>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = this.DbSet;
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        this.DbSet.Remove(entity);
    }

    /// <summary>
    /// Checks whether an entity exists in the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns <c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => await this.DbSet.AsNoTracking().AnyAsync(e => e.Id == id, cancellationToken);
}
