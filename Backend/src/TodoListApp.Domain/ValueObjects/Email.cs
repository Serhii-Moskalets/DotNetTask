using System.Text.RegularExpressions;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents a validated email address value object.
/// </summary>
public sealed partial record Email
{
    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    /// <summary>
    /// The maximum allowed length for a email.
    /// </summary>
    public const int MaxLength = 254;

    /// <summary>
    /// Gets the string representation of the email address.
    /// </summary>
    public string Value { get; } = string.Empty;

    private Email() { }

    private Email(string value) => this.Value = value;

    /// <summary>
    /// Creates a new <see cref="Email"/> instance after validating the format.
    /// </summary>
    /// <param name="value">The raw email string to validate.</param>
    /// <returns>A validated <see cref="Email"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown when the email is null, empty, or does not match the required format.
    /// </exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email cannot be empty.");
        }

        var trimmedEmail = value.Trim();

        if (trimmedEmail.Length > MaxLength)
        {
            throw new DomainException($"An email cannot contain more than {MaxLength} characters.");
        }

        if (!EmailRegex().IsMatch(trimmedEmail))
        {
            throw new DomainException("Invalid email format.");
        }

        return new Email(trimmedEmail.ToLowerInvariant());
    }

    /// <summary>
    /// Returns the string representation of the email address.
    /// </summary>
    /// <returns>The email address string.</returns>
    public override string ToString() => this.Value;
}
