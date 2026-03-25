using DotNetTask.Application.Abstractions.Interfaces.DotNetTaskDbContext;
using DotNetTask.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotNetTask.Infrastructure.Persistence.DatabaseContext;

/// <summary>
/// Represents the database context for the TodoList application.
/// Provides access to the database sets for all entities and applies their configurations.
/// </summary>
public class DotNetTaskDbContext : DbContext, IDotNetTaskDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotNetTaskDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public DotNetTaskDbContext(DbContextOptions<DotNetTaskDbContext> options)
        : base(options) { }

    /// <summary>
    /// Gets or sets the DbSet of task lists.
    /// </summary>
    public DbSet<TaskListEntity> TaskLists { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of tasks.
    /// </summary>
    public DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of tags.
    /// </summary>
    public DbSet<TagEntity> Tags { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of comments.
    /// </summary>
    public DbSet<CommentEntity> Comments { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of users.
    /// </summary>
    public DbSet<UserEntity> Users { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of user task accesses.
    /// </summary>
    public DbSet<UserTaskAccessEntity> UserTaskAccesses { get; set; }

    /// <summary>
    /// Configures the model by applying entity configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure the entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DotNetTaskDbContext).Assembly);

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IEnumerable<IMutableProperty> properties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

            foreach (IMutableProperty? property in properties)
            {
                property.SetColumnType("timestamp with time zone");
            }
        }
    }
}
