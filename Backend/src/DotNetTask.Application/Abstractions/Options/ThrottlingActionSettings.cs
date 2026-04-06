namespace DotNetTask.Application.Abstractions.Options;

/// <summary>
/// Represents the specific rate-limiting configuration for a single application action.
/// </summary>
public record ThrottlingActionSettings
{
    /// <summary>
    /// Gets the name of the action to which these throttling settings apply.
    /// </summary>
    public string ActionName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the duration of the rolling time window for the attempt count.
    /// </summary>
    public TimeSpan Window { get; init; }

    /// <summary>
    /// Gets the maximum number of permitted attempts allowed within the specified <see cref="Window"/>.
    /// </summary>
    public int MaxAttempts { get; init; }
}
