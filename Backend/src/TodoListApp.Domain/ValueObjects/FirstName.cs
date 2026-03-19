using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents a first name as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public sealed record FirstName
{
    /// <summary>
    /// The maximum allowed length for a first name.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Gets the string value of the person's first name.
    /// </summary>
    public string Value { get; } = string.Empty;

    private FirstName() { }

    private FirstName(string value) => this.Value = value;

    /// <summary>
    /// Creates a new <see cref="FirstName"/> instance with validation.
    /// </summary>
    /// <param name="value">The name string to be validated and trimmed.</param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown when the first name is null, empty or contain more than <see cref="MaxLength"/> characters.
    /// </exception>
    public static FirstName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(FirstNamePolicy.EmptyMessage);
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException(FirstNamePolicy.TooLongMessage);
        }

        return new FirstName(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the first name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
