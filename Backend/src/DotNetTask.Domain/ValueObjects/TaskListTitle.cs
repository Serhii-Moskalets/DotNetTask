using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;

namespace DotNetTask.Domain.ValueObjects;

/// <summary>
/// Represents the title of a task list.
/// Ensures that the task title is non-empty and does not exceed
/// the maximum allowed length defined by the domain.
/// </summary>
public sealed record TaskListTitle
{
    /// <summary>
    /// The maximum allowed length for a task list title.
    /// </summary>
    public const int MaxLength = 50;

    private TaskListTitle() { }

    private TaskListTitle(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the task list title.
    /// </summary>
    public string Value { get; } = string.Empty;

    /// <summary>
    /// Creates a new instance of <see cref="TaskListTitle"/> after validating
    /// that the provided value is not null, empty or whitespace,
    /// and doesn't exceed <see cref="MaxLength"/> characters.
    /// </summary>
    /// <param name="value">
    /// The raw task list title. Cannot be null, empty, or consist only of whitespace.
    /// Leading and trailing whitespace will be trimmed before validation.
    /// </param>
    /// <returns>
    /// A valid <see cref="TaskListTitle"/> instance containing the trimmed value.
    /// </returns>
    /// <exception cref="DomainException">
    /// Thrown when the value is null, empty, whitespace,
    /// or longer than <see cref="MaxLength"/>.
    /// </exception>
    public static TaskListTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(TaskListPolicy.EmptyMessage);
        }

        string trimmedValue = value.Trim();
        if (trimmedValue.Length > MaxLength)
        {
            throw new DomainException(TaskListPolicy.TooLongMessage);
        }

        return new TaskListTitle(trimmedValue);
    }

    /// <summary>
    /// Returns a string representation of the task list title.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
    {
        return this.Value;
    }
}
