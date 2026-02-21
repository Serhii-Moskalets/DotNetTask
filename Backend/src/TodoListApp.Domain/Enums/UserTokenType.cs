namespace TodoListApp.Domain.Enums;

/// <summary>
/// Represents types of user tokens used for various account-related operations.
/// </summary>
public enum UserTokenType
{
    /// <summary>
    /// Token used for verifying the user's email address.
    /// </summary>
    EmailVerification = 0,

    /// <summary>
    /// Token used for resetting the user's password.
    /// </summary>
    PasswordReset = 1,

    /// <summary>
    /// Token used for changing the user's email address.
    /// </summary>
    EmailChange = 2,

    /// <summary>
    /// Token used to revert a user's email address change.
    /// </summary>
    EmailChangeRevert = 3,
}
