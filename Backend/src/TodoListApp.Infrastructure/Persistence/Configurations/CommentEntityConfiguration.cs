using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="CommentEntity"/> entity.
/// Sets primary key, property constraints, and relationships.
/// </summary>
public class CommentEntityConfiguration : IEntityTypeConfiguration<CommentEntity>
{
    /// <summary>
    /// Configures the <see cref="CommentEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<CommentEntity> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        builder.Property(c => c.TaskId)
            .HasColumnName("task_id")
            .HasColumnType("uuid");

        builder.Property(c => c.Content)
            .HasConversion(c => c.Value, v => CommentContent.Create(v))
            .HasColumnName("content")
            .IsRequired()
            .HasMaxLength(CommentContent.MaxLength);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
