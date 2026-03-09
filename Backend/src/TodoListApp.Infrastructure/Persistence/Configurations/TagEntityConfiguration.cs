using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TagEntity"/> entity.
/// Sets primary key, property constraints, and relationships.
/// </summary>
public class TagEntityConfiguration : IEntityTypeConfiguration<TagEntity>
{
    /// <summary>
    /// Configures the <see cref="TagEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasConversion(t => t.Value, v => TagName.Create(v))
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(TagName.MaxLength);

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        builder.HasOne(t => t.User)
            .WithMany(t => t.Tags)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
