using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents the title of a task.
/// Ensures that the task title is non-empty and does not exceed
/// the maximum allowed length defined by the domain.
/// </summary>
public sealed record TaskTitle
{
    /// <summary>
    /// The maximum allowed length for a task title.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Gets the underlying string value of the task title.
    /// </summary>
    public string Value { get; } = string.Empty;

    private TaskTitle() { }

    private TaskTitle(string value) => this.Value = value;

    /// <summary>
    /// Creates a new instance of <see cref="TaskTitle"/> after validating
    /// that the provided value is not null, empty or whitespace,
    /// and doesn't exceed <see cref="MaxLength"/> characters.
    /// </summary>
    /// <param name="value">
    /// The raw task title. Cannot be null, empty, or consist only of whitespace.
    /// Leading and trailing whitespace will be trimmed before validation.
    /// </param>
    /// <returns>
    /// A valid <see cref="TaskTitle"/> instance containing the trimmed value.
    /// </returns>
    /// <exception cref="DomainException">
    /// Thrown when the value is null, empty, whitespace,
    /// or longer than <see cref="MaxLength"/>.
    /// </exception>
    public static TaskTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Task title cannot be empty.");
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException($"Task title cannot exceed {MaxLength} characters.");
        }

        return new TaskTitle(trimmedValue);
    }

    /// <summary>
    /// Creates a <see cref="TaskTitle"/> from the given string if it is not null or whitespace.
    /// Returns <c>null</c> if the input is null, empty, or consists only of whitespace.
    /// This is useful for optional title, e.g., when updating a task where the description might remain unchanged.
    /// </summary>
    /// <param name="value">
    /// The raw task title string. Can be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// A <see cref="TaskTitle"/> instance containing the trimmed value,
    /// or <c>null</c> if <paramref name="value"/> is null, empty, or whitespace.
    /// </returns>
    public static TaskTitle? CreateOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Create(value);

    /// <summary>
    /// Returns the string representation of the task title.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString() => this.Value;
}
