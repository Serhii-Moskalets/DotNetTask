using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Common;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides a base configuration for entities derived from <see cref="BaseEntity"/>.
/// Handles common mapping for identifiers, creation dates, and domain events.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being configured. Must inherit from <see cref="BaseEntity"/>.</typeparam>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Configures the base properties for the entity type.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    /// <remarks>
    /// This method sets the primary key to 'id', maps it to a UUID column,
    /// and ensures that domain events are not mapped to the database schema.
    /// </remarks>
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(e => e.CreatedDate)
            .HasColumnName("created_date");

        builder.Ignore(e => e.DomainEvents);
    }
}
