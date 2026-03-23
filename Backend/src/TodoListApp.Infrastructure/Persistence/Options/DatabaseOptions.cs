namespace TodoListApp.Infrastructure.Persistence.Options;

/// <summary>
/// Contains configuration settings for the database connection and resilience strategies.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// The name of the configuration section in appsettings.json.
    /// </summary>
    public const string SectionName = "DatabaseSettings";

    /// <summary>
    /// Gets the maximum number of retry attempts for transient database errors.
    /// Default value is 3.
    /// </summary>
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>
    /// Gets the delay in seconds between retry attempts.
    /// Default value is 5 seconds.
    /// </summary>
    public int MaxRetryDelaySeconds { get; init; } = 5;

    /// <summary>
    /// Gets the time in seconds to wait before terminating a command execution.
    /// Default value is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; init; } = 30;
}
