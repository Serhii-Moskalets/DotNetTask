namespace DotNetTask.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Defines the contract for requests that require rate limiting or throttling logic.
/// </summary>
public interface IThrottledRequest
{
    /// <summary>
    /// Gets the unique descriptor or name of the action being performed.
    /// </summary>
    string ActionName { get; }

    /// <summary>
    /// Retrieves a unique string representing the identity of the requester (e.g., IP address or User ID).
    /// </summary>
    /// <returns>A string used to differentiate the source of the request.</returns>
    string GetIdentity();
}
