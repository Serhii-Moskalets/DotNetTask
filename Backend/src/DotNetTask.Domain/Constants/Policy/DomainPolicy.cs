namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for defining domain policy requirements.
/// </summary>
public static class DomainPolicy
{
    /// <summary>
    /// Error message for business rule violations.
    /// </summary>
    public const string BusinessRuleViolationMessage = "Business rule violation";

    /// <summary>
    /// Error message when a resource is not found.
    /// </summary>
    public const string ResourceNotFoundMessage = "Resource not found";

    /// <summary>
    /// Error message when a server error occurs.
    /// </summary>
    public const string ServerErrorMessage = "Server error";

    /// <summary>
    /// Error message for unexpected errors.
    /// </summary>
    public const string UnexpectedErrorMessage = "An unexpected error occurred.";
}
