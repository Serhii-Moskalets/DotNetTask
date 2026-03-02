namespace TodoListApp.Application.Abstractions.Interfaces.Common;

/// <summary>
/// Defines a contract for generating various application-related URLs,
/// typically used for email notifications (e.g., confirmation links, password resets).
/// </summary>
public interface IUrlProvider
{
    /// <summary>
    /// Generates a link for user email confirmation.
    /// </summary>
    /// <param name="token">The security token generated for verification.</param>
    /// <returns>A fully qualified URL string for the email confirmation page.</returns>
    string GetEmailConfirmationLink(string token);

    /// <summary>
    /// Generates a link for password reset.
    /// </summary>
    /// <param name="token">The security token generated for the password reset operation.</param>
    /// <returns>A fully qualified URL string for the password reset page.</returns>
    string GetPasswordResetLink(string token);

    /// <summary>
    /// Generates a link to confirm an email address change.
    /// </summary>
    /// <param name="token">The security token generated for the email change operation.</param>
    /// <returns>A fully qualified URL string for the email change confirmation page.</returns>
    string GetEmailChangeLink(string token);

    /// <summary>
    /// Generates a link to revert an email address change.
    /// </summary>
    /// <param name="token">The security token generated to revert the email change operation.</param>
    /// <returns>A fully qualified URL string for the email change revert page.</returns>
    string GetEmailRevertLink(string token);
}
