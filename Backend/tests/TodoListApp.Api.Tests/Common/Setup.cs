namespace TodoListApp.Api.Tests.Common;

/// <summary>
/// Provides helper methods for setting up test environments.
/// </summary>
internal static class Setup
{
    /// <summary>
    /// Creates a new instance of <see cref="TestHttpContext"/> for unit testing.
    /// </summary>
    /// <returns>A new <see cref="TestHttpContext"/> instance.</returns>
    public static TestHttpContext CreateHttpContext() => new();
}
