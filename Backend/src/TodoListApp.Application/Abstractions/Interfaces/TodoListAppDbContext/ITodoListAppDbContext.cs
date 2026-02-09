using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Abstractions.Interfaces.TodoListAppDbContext;

/// <summary>
/// Represents the contract for the TodoList application database context.
/// Provides access to all entity sets and a method to persist changes asynchronously.
/// </summary>
public interface ITodoListAppDbContext
{
    /// <summary>
    /// Gets or sets the <see cref="DbSet{TaskListEntity}"/> representing task lists.
    /// </summary>
    DbSet<TaskListEntity> TaskLists { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TaskEntity}"/> representing tasks.
    /// </summary>
    DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TagEntity}"/> representing tags.
    /// </summary>
    DbSet<TagEntity> Tags { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{CommentEntity}"/> representing comments.
    /// </summary>
    DbSet<CommentEntity> Comments { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{UserEntity}"/> representing users.
    /// </summary>
    DbSet<UserEntity> Users { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{UserTaskAccessEntity}"/> representing user task access records.
    /// </summary>
    DbSet<UserTaskAccessEntity> UserTaskAccesses { get; set; }

    /// <summary>
    /// Gets the change tracker that provides access to change tracking information.
    /// </summary>
    ChangeTracker ChangeTracker { get; }

    /// <summary>
    /// Persists all changes made in the context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{Int32}"/> representing the asynchronous save operation, with the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
