using System.Text.RegularExpressions;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents user name as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public sealed partial record UserName
{
    /// <summary>
    /// The minimum allowed length for a username.
    /// </summary>
    public const int MinLength = 3;

    /// <summary>
    /// The maximum allowed length for a username.
    /// </summary>
    public const int MaxLength = 32;

    [GeneratedRegex(UserNamePolicy.FormatRegex)]
    private static partial Regex UserNameRegex();

    /// <summary>
    /// Gets the string value of the user name.
    /// </summary>
    public string Value { get; } = string.Empty;

    private UserName() { }

    private UserName(string value) => this.Value = value;

    /// <summary>
    /// Creates a new <see cref="UserName"/> instance with validation.
    /// </summary>
    /// <param name="value">The user name string to be validated and trimmed.</param>
    /// <returns>A validated <see cref="UserName"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the user name is null or empty.</exception>
    public static UserName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(UserNamePolicy.EmptyMessage);
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length is < MinLength or > MaxLength)
        {
            throw new DomainException(UserNamePolicy.LengthMessage);
        }

        if (!UserNameRegex().IsMatch(trimmedValue))
        {
            throw new DomainException(UserNamePolicy.InvalidCharactersMessage);
        }

        return new UserName(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the user name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
