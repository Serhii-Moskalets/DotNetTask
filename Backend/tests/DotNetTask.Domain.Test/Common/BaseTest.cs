namespace DotNetTask.Domain.Test.Common;

/// <summary>
/// Base class for all domain unit tests.
/// Provides a shared infrastructure for deterministic testing, such as frozen time.
/// </summary>
public abstract class BaseTest
{
    /// <summary>
    /// Gets the fake clock instance for the current test context.
    /// </summary>
    protected FakeClock Clock { get; } = new(TestTime.Now);
}
