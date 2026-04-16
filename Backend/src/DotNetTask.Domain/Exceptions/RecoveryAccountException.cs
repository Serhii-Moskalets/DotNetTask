using DotNetTask.Domain.Constants;

namespace DotNetTask.Domain.Exceptions;

/// <summary>
/// Represents an error that occurs during the account recovery process,
/// typically when an account is in a restricted state like pending deletion.
/// </summary>
public class RecoveryAccountException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoveryAccountException"/> class.
    /// </summary>
    public RecoveryAccountException()
        : base(UserPolicy.AccountPendingDeletionMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoveryAccountException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RecoveryAccountException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoveryAccountException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RecoveryAccountException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
