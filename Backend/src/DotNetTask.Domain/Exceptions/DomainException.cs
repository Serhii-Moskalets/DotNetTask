namespace DotNetTask.Domain.Exceptions;

/// <summary>
/// Represents a base exception type for domain-related errors.
/// </summary>
/// <remarks>
/// Domain exceptions are used to signal violations of business rules
/// or invalid operations within the domain layer.
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    public DomainException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
