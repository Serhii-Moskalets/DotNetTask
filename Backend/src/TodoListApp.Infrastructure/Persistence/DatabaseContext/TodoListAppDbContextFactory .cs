using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TodoListApp.Infrastructure.Persistence.DatabaseContext;

/// <summary>
/// Provides a design-time factory for creating instances of <see cref="TodoListAppDbContext"/>.
/// This factory is used by EF Core tools such as migrations when the application's
/// service provider is not available at design time.
/// </summary>
public class TodoListAppDbContextFactory : IDesignTimeDbContextFactory<TodoListAppDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="TodoListAppDbContext"/> using
    /// the connection string from <c>appsettings.json</c>.
    /// </summary>
    /// <param name="args">An array of arguments (not used in this implementation).</param>
    /// <returns>An instance of <see cref="TodoListAppDbContext"/> configured with Npgsql.</returns>
    public TodoListAppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<TodoListAppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TodoListAppDbContext(optionsBuilder.Options);
    }
}