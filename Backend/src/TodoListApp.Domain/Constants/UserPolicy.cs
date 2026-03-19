namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for general user-related policy messages and constraints.
/// </summary>
public static class UserPolicy
{
    /// <summary>
    /// The error message used when the provided user entity does not match the specified user identifier.
    /// </summary>
    public const string IdMismatchMessage = "User ID mismatch.";

    /// <summary>
    /// The error message used when a unique user identifier is missing from a request.
    /// </summary>
    public const string IdRequiredMessage = "User ID is required.";

    /// <summary>
    /// The error message when no profile data is provided for an update.
    /// </summary>
    public const string AtLeastOneFieldRequiredMessage = "At least one field (FirstName or LastName) must be provided.";

    /// <summary>
    /// The error message when email confirmation is attempted after the email has already been confirmed.
    /// </summary>
    public const string EmailAlreadyConfirmed = "Cannot update registration details after email is confirmed.";

    /// <summary>
    /// The error message when the account not found or deleted during a general search.
    /// </summary>
    public const string AccountNotFoundMessage = "Account not found. It may have been deleted.";

    /// <summary>
    /// The error message for unexpected failures when a logged-in user's record is missing in the database.
    /// </summary>
    public const string UnexpectedErrorMessage = "An unexpected error occurred while accessing your account information.";

    /// <summary>
    /// The error message when user authentication fails due to invalid credentials.
    /// </summary>
    public const string InvalidCredentialsMessage = "Invalid email or password.";

    /// <summary>
    /// The error message when a user tries to update their profile but the provided names are the same as the current ones.
    /// </summary>
    public const string NoChangesDetectedMessage = "No changes detected. The provided names are the same as your current ones.";
}
