using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents the textual content of a task comment.
/// Ensures that the comment content is non-empty and does not exceed
/// the maximum allowed length defined by the domain.
/// </summary>
public record CommentContent
{
    /// <summary>
    /// The maximum allowed length for a comment.
    /// </summary>
    public const int MaxLength = 4000;

    private CommentContent() { }

    private CommentContent(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the comment content.
    /// </summary>
    public string Value { get; private init; } = string.Empty;

    /// <summary>
    /// Creates a new instance of <see cref="CommentContent"/> after validating
    /// that the provided value is not null, empty, or whitespace,
    /// and does not exceed <see cref="MaxLength"/> characters.
    /// </summary>
    /// <param name="value">
    /// The raw comment text. Cannot be null, empty, or consist only of whitespace.
    /// Leading and trailing whitespace will be trimmed before validation.
    /// </param>
    /// <returns>
    /// A valid <see cref="CommentContent"/> instance containing the trimmed value.
    /// </returns>
    /// <exception cref="DomainException">
    /// Thrown when the value is null, empty, whitespace,
    /// or longer than <see cref="MaxLength"/>.
    /// </exception>
    public static CommentContent Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(CommentPolicy.EmptyMessage);
        }

        string trimmedValue = value.Trim();
        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException(CommentPolicy.TooLongMessage);
        }

        return new CommentContent(trimmedValue);
    }

    /// <summary>
    /// Returns the string representation of the comment content.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
    {
        return this.Value;
    }
}
