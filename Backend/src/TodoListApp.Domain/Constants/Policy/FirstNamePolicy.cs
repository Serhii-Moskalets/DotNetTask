using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining first name policy requirements.
/// </summary>
public static class FirstNamePolicy
{
    /// <summary>
    /// Error message for null or empty first name.
    /// </summary>
    public const string EmptyMessage = "First name is required.";

    /// <summary>
    /// Error message when the first name exceeds the maximum allowed length.
    /// </summary>
    public static readonly string TooLongMessage = $"First name cannot be longer than {FirstName.MaxLength} characters.";
}
