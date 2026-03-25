namespace DotNetTask.Application.Common.Dtos;

/// <summary>
/// Data Transfer Object (DTO) representing a brief view of a user.
/// </summary>
public class UserBriefDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the firts name of the user.
    /// </summary>
    public string FirstName { get; init; } = null!;

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public string UserName { get; init; } = null!;

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    public string Email { get; init; } = null!;
}
