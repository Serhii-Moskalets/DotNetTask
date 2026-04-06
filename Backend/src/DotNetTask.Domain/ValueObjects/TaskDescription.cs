using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents a task description as a value object to ensure
/// consistent formatting and validation across the domain.
/// </summary>
public record TaskDescription
{
    /// <summary>
    /// The maximum allowed length for a task description.
    /// </summary>
    public const int MaxLength = 1000;

    private TaskDescription() { }

    private TaskDescription(string value) => this.Value = value;

    /// <summary>
    /// Gets the string value of the task description.
    /// </summary>
    public string Value { get; init; } = null!;

    /// <summary>
    /// Creates a new instance of <see cref="TaskDescription"/> after validating
    /// that the provided value is not null, empty or whitespace,
    /// and doesn't exceed <see cref="MaxLength"/> characters.
    /// </summary>
    /// <param name="value">
    /// The raw task description. Cannot be null, empty, or consist only of whitespace.
    /// Leading and trailing whitespace will be trimmed before validation.
    /// </param>
    /// <returns>A valid <see cref="TaskDescription"/> instance containing the trimmed value.</returns>
    /// <exception cref="DomainException">Thrown when the value exceeds <see cref="MaxLength"/>.</exception>
    public static TaskDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(TaskPolicy.EmptyDescriptionMessage);
        }

        string trimmedValue = value.Trim();
        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException(TaskPolicy.TooLongDescriptionMessage);
        }

        return new TaskDescription(trimmedValue);
    }

    /// <summary>
    /// Creates a <see cref="TaskDescription"/> from the given string if it is not null or whitespace.
    /// Returns <c>null</c> if the input is null, empty, or consists only of whitespace.
    /// This is useful for optional descriptions, e.g., when updating a task where the description
    /// might be removed.
    /// </summary>
    /// <param name="value">
    /// The raw task description string. Can be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// A <see cref="TaskDescription"/> instance containing the trimmed value,
    /// or <c>null</c> if <paramref name="value"/> is null, empty, or whitespace.
    /// </returns>
    public static TaskDescription? CreateOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : Create(value);

    /// <summary>
    /// Returns the string representation of the task descriptions.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
