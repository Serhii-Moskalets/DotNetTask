namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for pagination policy requirements.
/// </summary>
public static class PaginationPolicy
{
    /// <summary>
    /// The error message when the requested page number is less than one.
    /// </summary>
    public const string PageMinMessage = "Page must be at least 1.";

    /// <summary>
    /// The error message when the page size is outside the allowed range (1-100).
    /// </summary>
    public const string PageSizeRangeMessage = "Page size must be between 1 and 100.";
}
