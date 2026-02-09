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
    /// <param name="email">The email address to be confirmed.</param>
    /// <param name="token">The security token generated for verification.</param>
    /// <returns>A fully qualified URL string for the email confirmation page.</returns>
    string GetEmailConfirmationLink(string email, string token);

    /// <summary>
    /// Generates a link for password reset.
    /// </summary>
    /// <param name="email">The email address of the user requesting the reset.</param>
    /// <param name="token">The security token generated for the password reset operation.</param>
    /// <returns>A fully qualified URL string for the password reset page.</returns>
    string GetPasswordResetLink(string email, string token);

    /// <summary>
    /// Generates a link to confirm an email address change.
    /// </summary>
    /// <param name="email">The current email address of the user.</param>
    /// <param name="token">The security token generated for the email change operation.</param>
    /// <param name="newEmail">The new email address that needs to be confirmed.</param>
    /// <returns>A fully qualified URL string for the email change confirmation page.</returns>
    string GetEmailChangeLink(string email, string token, string newEmail);
}
