using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents the name of a task tag.
/// Ensures that the tag name is non-empty and does not exceed
/// the maximum allowed length defined by the domain.
/// </summary>
public record TagName
{
    /// <summary>
    /// The maximum allowed length for a tag name.
    /// </summary>
    public const int MaxLength = 50;

    private TagName() { }

    private TagName(string value) => this.Value = value;

    /// <summary>
    /// Gets the underlying string value of the tag name.
    /// </summary>
    public string Value { get; } = string.Empty;

    /// <summary>
    /// Creates a new instance of <see cref="TagName"/> after validating
    /// that the provided value is not null, empty or whitespace,
    /// and doesn't exceed <see cref="MaxLength"/> characters.
    /// </summary>
    /// <param name="value">
    /// The raw tag name. Cannot be null, empty, or consist only of whitespace.
    /// Leading and trailing whitespace will be trimmed before validation.
    /// </param>
    /// <returns>
    /// A valid <see cref="TagName"/> instance containing the trimmed value.
    /// </returns>
    /// <exception cref="DomainException">
    /// Thrown when the value is null, empty, whitespace,
    /// or longer than <see cref="MaxLength"/>.
    /// </exception>
    public static TagName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(TagPolicy.EmptyMessage);
        }

        string trimmedValue = value.Trim();
        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException(TagPolicy.TooLongMessage);
        }

        return new TagName(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the tag name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
