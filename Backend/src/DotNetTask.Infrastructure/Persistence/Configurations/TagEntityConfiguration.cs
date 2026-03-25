using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TagEntity"/> entity.
/// Sets primary key, property constraints, and relationships.
/// </summary>
public class TagEntityConfiguration : BaseEntityConfiguration<TagEntity>
{
    /// <summary>
    /// Configures the <see cref="TagEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public override void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("tags");

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

        builder.Navigation(t => t.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
