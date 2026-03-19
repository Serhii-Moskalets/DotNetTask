namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for common validation policies like pagination.
/// </summary>
public static class CommonPolicy
{
    /// <summary>
    /// The error message when the requested page number is less than one.
    /// </summary>
    public const string PageMinMessage = "Page must be at least 1.";

    /// <summary>
    /// The error message when the page size is outside the allowed range (1-100).
    /// </summary>
    public const string PageSizeRangeMessage = "Page size must be between 1 and 100.";

    /// <summary>
    /// Maximum allowed search text length.
    /// </summary>
    public const int MaxSearchTextLength = 100;

    /// <summary>
    /// Error message when search text is too long.
    /// </summary>
    public static readonly string SearchTextTooLongMessage = $"Search text cannot exceed {MaxSearchTextLength} characters.";
}