using MediatR;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.TodoListAppDbContext;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Domain.Common;

namespace TodoListApp.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Represents a Unit of Work for managing repositories and saving changes to the database.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ITodoListAppDbContext _dbContext;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class with all required repositories.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="publisher">The mediator publisher used to dispatch domain events.</param>
    /// <param name="commentRepository">The repository for comments.</param>
    /// <param name="tagRepository">The repository for tags.</param>
    /// <param name="taskListRepository">The repository for task lists.</param>
    /// <param name="taskRepository">The repository for tasks.</param>
    /// <param name="userRepository">The repository for users.</param>
    /// <param name="userTaskAccessRepository">The repository for user task access management.</param>
    public UnitOfWork(
        ITodoListAppDbContext context,
        IPublisher publisher,
        ICommentRepository commentRepository,
        ITagRepository tagRepository,
        ITaskListRepository taskListRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        IUserTaskAccessRepository userTaskAccessRepository)
    {
        this._dbContext = context;
        this._publisher = publisher;
        this.Comments = commentRepository;
        this.Tags = tagRepository;
        this.TaskLists = taskListRepository;
        this.Tasks = taskRepository;
        this.Users = userRepository;
        this.UserTaskAccesses = userTaskAccessRepository;
    }

    /// <summary>
    /// Gets the repository for managing comments.
    /// </summary>
    public ICommentRepository Comments { get; }

    /// <summary>
    /// Gets the repository for managing tags.
    /// </summary>
    public ITagRepository Tags { get; }

    /// <summary>
    /// Gets the repository for managing task lists.
    /// </summary>
    public ITaskListRepository TaskLists { get; }

    /// <summary>
    /// Gets the repository for managing tasks.
    /// </summary>
    public ITaskRepository Tasks { get; }

    /// <summary>
    /// Gets the repository for managing users.
    /// </summary>
    public IUserRepository Users { get; }

    /// <summary>
    /// Gets the repository for managing user task accesses.
    /// </summary>
    public IUserTaskAccessRepository UserTaskAccesses { get; }

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await this._dbContext.SaveChangesAsync(cancellationToken);

        await this.DispatchDomainEventsAsync(cancellationToken);

        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = this._dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .Select(x => x.Entity)
            .ToList();

        if (domainEntities.Count == 0)
        {
            return;
        }

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await this._publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
