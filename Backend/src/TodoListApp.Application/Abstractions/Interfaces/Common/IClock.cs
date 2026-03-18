namespace TodoListApp.Application.Abstractions.Interfaces.Common;

/// <summary>
/// Defines a provider for date and time operations to enable testability.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current date and time in Coordinated Universal Time (UTC).
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> representing the current moment in UTC.
    /// </value>
    DateTime UtcNow { get; }
}
