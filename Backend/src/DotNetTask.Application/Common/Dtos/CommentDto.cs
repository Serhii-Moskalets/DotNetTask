namespace DotNetTask.Application.Common.Dtos;

/// <summary>
/// Data Transfer Object (DTO) representing a comment in the application layer.
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Gets the unique identifier of the comment.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the text content of the comment.
    /// </summary>
    public string Content { get; init; } = null!;

    /// <summary>
    /// Gets the date and time when the comment was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the owner of the comment.
    /// Uses <see cref="UserBriefDto"/> to provide only the essential user information.
    /// </summary>
    public UserBriefDto User { get; init; } = null!;
}
