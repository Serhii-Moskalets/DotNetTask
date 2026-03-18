using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining last name policy requirements.
/// </summary>
public static class LastNamePolicy
{
    /// <summary>Error message for null or empty last name.</summary>
    public const string EmptyMessage = "Last name is required.";

    /// <summary>Error message when the last name exceeds the maximum allowed length.</summary>
    public static readonly string TooLongMessage = $"Last name cannot be longer than {LastName.MaxLength} characters.";
}
