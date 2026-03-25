namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for common validation policies like pagination.
/// </summary>
public static class CommonPolicy
{
    /// <summary>
    /// The name of the default database connection string in the configuration.
    /// </summary>
    public const string DataBaseConnectionString = "DefaultConnection";

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
    /// The error message when user id is invalid.
    /// </summary>
    public const string InvalidUserIdentityMessage = "Invalid user identity.";

    /// <summary>
    /// The error message when search text is too long.
    /// </summary>
    public static readonly string SearchTextTooLongMessage = $"Search text cannot exceed {MaxSearchTextLength} characters.";

    /// <summary>
    /// The error message when the default database connection string is missing in the configuration.
    /// </summary>
    public static readonly string MissingConnectionStringMessage = $"Connection string '{DataBaseConnectionString}' not found.";
}
