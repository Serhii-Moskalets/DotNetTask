using TodoListApp.Domain.Common;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a tag that can be associated with tasks and owned by a user.
/// </summary>
public class TagEntity : BaseEntity
{
    private readonly HashSet<TaskEntity> _tasks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TagEntity"/> class.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <param name="userId">The ID of the user who created the tag.</param>
    public TagEntity(TagName name, Guid userId)
    {
        this.Name = name;
        this.UserId = userId;
    }

    private TagEntity() { }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    public TagName Name { get; private init; } = null!;

    /// <summary>
    /// Gets the ID of the user who owns this tag.
    /// </summary>
    public Guid UserId { get; private init; }

    /// <summary>
    /// Gets the user who owns this tag.
    /// </summary>
    public virtual UserEntity User { get; private init; } = null!;

    /// <summary>
    /// Gets the collection of tasks associated with this tag.
    /// </summary>
    public virtual IReadOnlyCollection<TaskEntity> Tasks => this._tasks;
}
