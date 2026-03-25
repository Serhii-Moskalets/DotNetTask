namespace DotNetTask.Application.Common.Dtos;

/// <summary>
/// Data Transfer Object (DTO) representing a Tag.
/// Used to transfer tag data between application layers without exposing the domain entity directly.
/// </summary>
public class TagDto
{
    /// <summary>
    /// Gets the unique identifier of the tag.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the identifier of the user who owns the tag.
    /// </summary>
    public Guid UserId { get; init; }
}
