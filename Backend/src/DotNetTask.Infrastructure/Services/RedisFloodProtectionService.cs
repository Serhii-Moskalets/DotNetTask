using DotNetTask.Application.Abstractions.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace DotNetTask.Infrastructure.Services;

/// <summary>
/// An implementation of <see cref="IFloodProtectionService"/> that uses
/// <see cref="IDistributedCache"/> (typically backed by Redis) to track request attempts.
/// </summary>
public class RedisFloodProtectionService : IFloodProtectionService
{
    private readonly IDatabase _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisFloodProtectionService"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    public RedisFloodProtectionService(IConnectionMultiplexer redis)
        => this._db = redis.GetDatabase();

    /// <summary>
    /// Evaluates if an action is allowed by checking and incrementing a counter in the distributed cache.
    /// </summary>
    /// <param name="identity">The unique identifier for the requester.</param>
    /// <param name="action">The name of the action being throttled.</param>
    /// <param name="maxAttempts">The maximum number of allowed attempts.</param>
    /// <param name="window">The expiration time for the rate limit counter.</param>
    /// <returns>
    /// <see langword="true"/> if the current attempt count is below <paramref name="maxAttempts"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method uses a cache key format of <c>ratelimit:{action}:{identity}</c>.
    /// </remarks>
    public async Task<bool> IsAllowedAsync(string identity, string action, int maxAttempts, TimeSpan window)
    {
        string key = $"ratelimit:{action}:{identity}";
        long count = await this._db.StringIncrementAsync(key);

        if (count == 1)
        {
            await this._db.KeyExpireAsync(key, window);
        }

        return count <= maxAttempts;
    }
}
