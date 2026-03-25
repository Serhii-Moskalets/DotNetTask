using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Test.Helpers;

/// <summary>
/// Provides a factory to create instances of <see cref="DotNetTaskDbContext"/>
/// configured to use an in-memory database for testing purposes.
/// </summary>
public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates a new <see cref="DotNetTaskDbContext"/> instance
    /// using a unique in-memory database.
    /// </summary>
    /// <returns>A <see cref="DotNetTaskDbContext"/> configured for in-memory usage.</returns>
    public static DotNetTaskDbContext Create()
    {
        DbContextOptions<DotNetTaskDbContext> options = new DbContextOptionsBuilder<DotNetTaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DotNetTaskDbContext(options);
    }
}
