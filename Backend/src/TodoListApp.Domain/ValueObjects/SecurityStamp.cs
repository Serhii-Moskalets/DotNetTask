using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.ValueObjects;

/// <summary>
/// Represents a unique security identifier used to track changes in user credentials
/// and invalidate active sessions.
/// </summary>
public record SecurityStamp
{
    /// <summary>
    /// Gets the raw string value of the security stamp.
    /// </summary>
    public string Value { get; init; } = null!;

    private SecurityStamp() { }

    private SecurityStamp(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new DomainException("Invalid security identifier.");
        }

        this.Value = value;
    }

    /// <summary>
    /// Generates a new unique security stamp using a GUID.
    /// </summary>
    /// <returns>A new <see cref="SecurityStamp"/> instance.</returns>
    public static SecurityStamp New() => new(Guid.NewGuid().ToString());

    /// <summary>
    /// Creates a security stamp from an existing string value.
    /// </summary>
    /// <param name="value">The security stamp string.</param>
    /// <returns>A <see cref="SecurityStamp"/> instance representing the provided value.</returns>
    public static SecurityStamp Create(string value) => new(value);
}
