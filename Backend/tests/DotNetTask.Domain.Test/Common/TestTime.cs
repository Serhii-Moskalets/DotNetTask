namespace DotNetTask.Domain.Test.Common;

/// <summary>
/// Provides a consistent and frozen time point for unit tests to ensure deterministic behavior.
/// </summary>
public static class TestTime
{
    /// <summary>
    /// A fixed point in time used as 'now' across domain tests.
    /// Value: 2026-01-01 12:00:00 UTC.
    /// </summary>
    public static readonly DateTime Now = new(2026, 01, 01, 12, 00, 00);
}
