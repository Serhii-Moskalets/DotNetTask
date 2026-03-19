using TodoListApp.Domain.Constants;

namespace TodoListApp.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access protected resources
/// but is required to change their password first.
/// </summary>
public class PasswordChangeRequiredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordChangeRequiredException"/> class.
    /// </summary>
    public PasswordChangeRequiredException()
        : base(UserPolicy.MustChangePasswordMessage)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordChangeRequiredException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public PasswordChangeRequiredException(string message)
        : base(message)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordChangeRequiredException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PasswordChangeRequiredException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
