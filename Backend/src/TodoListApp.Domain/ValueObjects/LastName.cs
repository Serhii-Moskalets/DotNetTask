using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents a last name as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public sealed record LastName
{
    /// <summary>
    /// Gets the string value of the person's last name.
    /// </summary>
    public string Value { get; } = null!;

    private LastName() { }

    private LastName(string value) => this.Value = value;

    /// <summary>
    /// Creates a new <see cref="LastName"/> instance with validation.
    /// </summary>
    /// <param name="value">The name string to be validated and trimmed.</param>
    /// <returns>A validated <see cref="LastName"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown when the last name contain more than 30 characters.
    /// </exception>
    public static LastName? Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > 30)
        {
            throw new DomainException("Last name cannot contain more than 30 characters.");
        }

        return new LastName(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the last name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
