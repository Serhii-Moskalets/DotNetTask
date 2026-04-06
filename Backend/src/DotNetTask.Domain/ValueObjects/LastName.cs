using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents a last name as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public sealed record LastName
{
    /// <summary>
    /// The maximum allowed length for a last name.
    /// </summary>
    public const int MaxLength = 100;

    private LastName() { }

    private LastName(string value) => this.Value = value;

    /// <summary>
    /// Gets the string value of the person's last name.
    /// </summary>
    public string Value { get; } = null!;

    /// <summary>
    /// Creates a new <see cref="LastName"/> instance with validation.
    /// </summary>
    /// <param name="value">The name string to be validated and trimmed.</param>
    /// <returns>A validated <see cref="LastName"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown when the last name is null, empty, or exceeds <see cref="MaxLength"/> characters.
    /// </exception>
    public static LastName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(LastNamePolicy.EmptyMessage);
        }

        string trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException(LastNamePolicy.TooLongMessage);
        }

        return new LastName(trimmedValue);
    }

    /// <summary>
    /// Creates an optional LastName instance from the specified string value.
    /// </summary>
    /// <remarks>This method is useful for scenarios where a last name may or may not be provided, allowing
    /// for a clean handling of optional values.</remarks>
    /// <param name="value">The string value representing the last name. If the value is null or consists only of white-space characters,
    /// the method returns null.</param>
    /// <returns>An instance of LastName if the value is valid; otherwise, null.</returns>
    public static LastName? CreateOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : Create(value);

    /// <summary>
    /// Returns the string representation of the last name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
