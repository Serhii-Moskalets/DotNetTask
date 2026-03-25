using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents password hash as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public sealed record PasswordHash
{
    private PasswordHash() { }

    private PasswordHash(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the string value of the password hash.
    /// </summary>
    public string Value { get; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="PasswordHash"/> instance with validation.
    /// </summary>
    /// <param name="value">The password hash string to be validated and trimmed.</param>
    /// <returns>A validated <see cref="PasswordHash"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the password hasр is null or empty.</exception>
    public static PasswordHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(SecurityPolicy.HashEmptyMessage);
        }

        string trimmedValue = value.Trim();

        return new PasswordHash(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the password hash.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
    {
        return this.Value;
    }
}
