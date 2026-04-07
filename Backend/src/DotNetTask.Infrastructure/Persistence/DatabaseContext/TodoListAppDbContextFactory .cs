using DotNetTask.Domain.Constants;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DotNetTask.Infrastructure.Persistence.DatabaseContext;

/// <summary>
/// Provides a design-time factory for creating instances of <see cref="DotNetTaskDbContext"/>.
/// This factory is used by EF Core tools such as migrations when the application's
/// service provider is not available at design time.
/// </summary>
public class DotNetTaskDbContextFactory : IDesignTimeDbContextFactory<DotNetTaskDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="DotNetTaskDbContext"/> using
    /// the connection string from <c>appsettings.json</c>.
    /// </summary>
    /// <param name="args">An array of arguments (not used in this implementation).</param>
    /// <returns>An instance of <see cref="DotNetTaskDbContext"/> configured with Npgsql.</returns>
    public DotNetTaskDbContext CreateDbContext(string[] args)
    {
        string basePath = Directory.GetCurrentDirectory();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        string connectionString = configuration.GetConnectionString(CommonPolicy.DataBaseConnectionString)
                ?? throw new InvalidOperationException(CommonPolicy.MissingConnectionStringMessage);

        DbContextOptionsBuilder<DotNetTaskDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql(connectionString);

        return new DotNetTaskDbContext(optionsBuilder.Options);
    }
}