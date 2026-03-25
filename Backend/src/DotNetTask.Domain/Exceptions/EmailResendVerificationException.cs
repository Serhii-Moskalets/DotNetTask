using DotNetTask.Domain.Constants;

namespace DotNetTask.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access protected resources
/// but is required to confirm your email first.
/// </summary>
public class EmailResendVerificationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailResendVerificationException"/> class.
    /// </summary>
    public EmailResendVerificationException()
        : base(UserPolicy.EmailIsNotConfirmedMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailResendVerificationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public EmailResendVerificationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailResendVerificationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EmailResendVerificationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
