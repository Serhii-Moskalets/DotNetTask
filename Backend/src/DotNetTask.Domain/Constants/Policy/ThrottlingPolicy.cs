namespace DotNetTask.Domain.Constants.Policy;

/// <summary>
/// Provides centralized constant values and messages used for throttling and rate-limiting policies.
/// </summary>
public static class ThrottlingPolicy
{
    /// <summary>
    /// The default error message returned to a user when they have exceeded the allowed number of attempts.
    /// </summary>
    public const string TooManyAttemptsMessage = "Too many attempts. Please try later.";

    /// <summary>
    /// The exception message thrown when a throttled request's response type is incompatible with the expected result type.
    /// </summary>
    public const string InvalidOperationMessage = "hrottled request must return Result<T>";
}
