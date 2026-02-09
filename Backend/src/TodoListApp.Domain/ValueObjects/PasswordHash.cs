using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents password hash as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public sealed record PasswordHash
{
    /// <summary>
    /// Gets the string value of the password hash.
    /// </summary>
    public string Value { get; } = string.Empty;

    private PasswordHash() { }

    private PasswordHash(string value) => this.Value = value;

    /// <summary>
    /// Creates a new <see cref="PasswordHash"/> instance with validation.
    /// </summary>
    /// <param name="value">The user name string to be validated and trimmed.</param>
    /// <returns>A validated <see cref="PasswordHash"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the user name is null or empty.</exception>
    public static PasswordHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("The password hash cannot be empty.");
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < 60)
        {
            throw new DomainException("Invalid password hash format.");
        }

        return new PasswordHash(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the user name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
