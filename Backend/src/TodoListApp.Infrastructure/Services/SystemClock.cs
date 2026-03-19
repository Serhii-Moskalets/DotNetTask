using TodoListApp.Application.Abstractions.Interfaces.Common;

namespace TodoListApp.Infrastructure.Services;

/// <summary>
/// Provides access to the system clock using standard .NET date and time utilities.
/// </summary>
public class SystemClock : IClock
{
    /// <inheritdoc />
    /// <remarks>
    /// This implementation returns the current system time in UTC.
    /// </remarks>
    public DateTime UtcNow => DateTime.UtcNow;
}
