using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Common;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a tag that can be associated with tasks and owned by a user.
/// </summary>
[Table("tags")]
public class TagEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagEntity"/> class.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <param name="userId">The ID of the user who created the tag.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="name"/> is null, empty, or consists only of white-space characters
    /// or exceed 50 characters.
    /// </exception>
    public TagEntity(string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Tag name cannot be empty.");
        }

        if (name.Length > 50)
        {
            throw new DomainException("Tag name cannot exceed 50 characters.");
        }

        this.Name = name.Trim();
        this.UserId = userId;
    }

    private TagEntity() { }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    [Column("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the ID of the user who owns this tag.
    /// </summary>
    [Column("user_id")]
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the user who owns this tag.
    /// </summary>
    public virtual UserEntity User { get; init; } = null!;

    /// <summary>
    /// Gets the collection of tasks associated with this tag.
    /// </summary>
    public virtual ICollection<TaskEntity> Tasks { get; init; } = new HashSet<TaskEntity>();
}
