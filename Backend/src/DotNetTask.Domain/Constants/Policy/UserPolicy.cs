namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for general user-related policy messages and constraints.
/// </summary>
public static class UserPolicy
{
    /// <summary>
    /// Represents the default number of days after which an item should be considered for deletion.
    /// </summary>
    public const int DeletionDelayInDays = 30;

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
    public const string EmailAlreadyConfirmedMessage = "Cannot update registration details after email is confirmed.";

    /// <summary>
    /// The title for when email confirmation is required.
    /// </summary>
    public const string EmailNotConfirmedTitle = "Email Not Confirmed";

    /// <summary>
    /// The error message when email confirmation is required but hasn't been completed.
    /// </summary>
    public const string EmailIsNotConfirmedMessage = "Email isn't confirmed.";

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

    /// <summary>
    /// The title for when a session has expired.
    /// </summary>
    public const string SessionExpiredTitle = "Unauthorized";

    /// <summary>
    /// The error message when session has expired.
    /// </summary>
    public const string SessionExpiredMessage = "Session has expired. Please login again.";

    /// <summary>
    /// The title for when a user must change their password.
    /// </summary>
    public const string MustChangePasswordTitle = "Password Change Required";

    /// <summary>
    /// The error message when a user must change their password before accessing other resources.
    /// </summary>
    public const string MustChangePasswordMessage = "You must change your password before accessing other resources.";

    /// <summary>
    /// The error message when a user is not active and tries to access resources that require an active account.
    /// </summary>
    public const string IsNotActiveMessage = "User is not active";

    /// <summary>
    /// The error message when a user is active and tries to access resources that require an inactive account.
    /// </summary>
    public const string IsActiveMessage = "User is active";

    /// <summary>
    /// The error message when a user tries to restore an account after the deletion period has expired.
    /// </summary>
    public const string DeletionPeriodExpiredMessage = "Too late to restore account";

    /// <summary>
    /// The title for when an account is pending deletion.
    /// </summary>
    public const string AccountPendingDeletionTitle = "Account Pending Deletion";

    /// <summary>
    /// The error message when a user tries to access resources while their account is pending deletion.
    /// </summary>
    public const string AccountPendingDeletionMessage = "Account is pending deletion. Please contact support if you wish to restore your account.";

    /// <summary>
    /// The error message when the status value used to indicate an unknown or invalid user state.
    /// </summary>
    public const string InvalidUserStatus = "Unknown user status";
}
