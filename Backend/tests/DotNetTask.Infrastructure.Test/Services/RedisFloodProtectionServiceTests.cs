using DotNetTask.Infrastructure.Services;
using FluentAssertions;
using Moq;
using StackExchange.Redis;

namespace DotNetTask.Infrastructure.Test.Services;

/// <summary>
/// Contains unit tests for the <see cref="RedisFloodProtectionService"/> to ensure
/// rate-limiting logic correctly interacts with the distributed cache.
/// </summary>
public class RedisFloodProtectionServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _dbMock;
    private readonly RedisFloodProtectionService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisFloodProtectionServiceTests"/> class.
    /// </summary>
    public RedisFloodProtectionServiceTests()
    {
        this._redisMock = new Mock<IConnectionMultiplexer>();
        this._dbMock = new Mock<IDatabase>();

        this._redisMock
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(this._dbMock.Object);
        this._service = new RedisFloodProtectionService(this._redisMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="RedisFloodProtectionService.IsAllowedAsync"/> returns <see langword="true"/>
    /// and initializes the cache counter when no previous attempts are recorded for the identity.
    /// </summary>
    /// <returns>>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IsAllowedAsync_ShouldReturnTrue_WhenNoPreviousAttemptsExist()
    {
        // Arrange
        string identity = "user-1";
        string action = "login";
        int maxAttempts = 3;
        TimeSpan window = TimeSpan.FromMinutes(1);
        string key = $"ratelimit:{action}:{identity}";

        this._dbMock.Setup(x => x.StringIncrementAsync(key, 1, CommandFlags.None))
          .ReturnsAsync(1);

        // Act
        bool result = await this._service.IsAllowedAsync(identity, action, maxAttempts, window);

        // Assert
        result.Should().BeTrue();

        this._dbMock.Verify(
            x => x.StringIncrementAsync(key, 1, CommandFlags.None),
            Times.Once);

        this._dbMock.Verify(
            x => x.KeyExpireAsync(key, window, ExpireWhen.Always, CommandFlags.None),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="RedisFloodProtectionService.IsAllowedAsync"/> returns <see langword="false"/>
    /// and does not increment the counter when the maximum allowed attempts have already been reached.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IsAllowedAsync_ShouldReturnFalse_WhenMaxAttemptsReached()
    {
        // Arrange
        string identity = "user-2";
        string action = "reset-password";
        int maxAttempts = 2;
        RedisKey key = $"ratelimit:{action}:{identity}";

        this._dbMock.Setup(x => x.StringIncrementAsync(key, 1, CommandFlags.None))
              .ReturnsAsync(3);

        // Act
        bool result = await this._service.IsAllowedAsync(identity, action, maxAttempts, TimeSpan.FromMinutes(1));

        // Assert
        result.Should().BeFalse();

        this._dbMock.Verify(
            x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Never);
    }
}
