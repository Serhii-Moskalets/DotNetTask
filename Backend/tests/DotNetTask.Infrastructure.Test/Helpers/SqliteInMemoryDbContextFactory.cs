using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Test.Helpers;

/// <summary>
/// Factory for creating an in-memory SQLite database context
/// for integration and repository tests.
/// </summary>
/// <remarks>
/// Uses a persistent in-memory SQLite connection to ensure
/// relational behaviors (constraints, foreign keys, transactions)
/// are supported during testing.
/// </remarks>
public static class SqliteInMemoryDbContextFactory
{
    /// <summary>
    /// Creates and initializes a new <see cref="DotNetTaskDbContext"/>
    /// backed by an in-memory SQLite database.
    /// </summary>
    /// <returns>
    /// A fully initialized <see cref="DotNetTaskDbContext"/> instance
    /// with the database schema created.
    /// </returns>
    public static DotNetTaskDbContext Create()
    {
        SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        DbContextOptions<DotNetTaskDbContext> options = new DbContextOptionsBuilder<DotNetTaskDbContext>()
            .UseSqlite(connection)
            .Options;

        DotNetTaskDbContext context = new DotNetTaskDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
