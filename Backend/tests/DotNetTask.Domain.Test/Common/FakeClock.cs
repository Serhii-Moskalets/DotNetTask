using DotNetTask.Application.Abstractions.Interfaces.Common;

namespace DotNetTask.Domain.Test.Common;

/// <summary>
/// A controllable clock implementation for unit testing.
/// </summary>
public class FakeClock : IClock
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeClock"/> class with a specific starting time.
    /// </summary>
    /// <param name="initialTime">The starting time for the test scenario.</param>
    public FakeClock(DateTime initialTime)
    {
        this.UtcNow = initialTime;
    }

    /// <summary>
    /// Gets the current mock UTC time.
    /// </summary>
    public DateTime UtcNow { get; private set; }

    /// <summary>
    /// Manually advances the clock by a specified duration.
    /// </summary>
    /// <param name="timeSpan">The amount of time to move forward.</param>
    public void Advance(TimeSpan timeSpan)
    {
        this.UtcNow = this.UtcNow.Add(timeSpan);
    }
}
