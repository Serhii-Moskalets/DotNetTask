namespace DotNetTask.Domain.Enums;

/// <summary>
/// Defines the various states a user account can be in.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// The account has been created but not yet verified.
    /// </summary>
    Unconfirmed = 0,

    /// <summary>
    /// The account is fully verified and active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The user has requested account deletion, and it is awaiting permanent removal.
    /// </summary>
    PendingDeletion = 2,
}
