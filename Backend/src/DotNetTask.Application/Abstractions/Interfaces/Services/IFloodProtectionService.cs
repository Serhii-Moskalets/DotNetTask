namespace DotNetTask.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Provides a service for evaluating whether a specific action should be permitted
/// based on rate-limiting and flood protection rules.
/// </summary>
public interface IFloodProtectionService
{
    /// <summary>
    /// Determines if a specific user or identity is allowed to perform a given action
    /// within a defined time window.
    /// </summary>
    /// <param name="identity">The unique identifier of the user or client making the request.</param>
    /// <param name="action">The name or key of the operation being attempted.</param>
    /// <param name="maxAttempts">The maximum number of attempts allowed before being throttled.</param>
    /// <param name="window">The duration of the rolling time frame for the attempt count.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains <see langword="true"/> if the action is allowed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> IsAllowedAsync(string identity, string action, int maxAttempts, TimeSpan window);
}
