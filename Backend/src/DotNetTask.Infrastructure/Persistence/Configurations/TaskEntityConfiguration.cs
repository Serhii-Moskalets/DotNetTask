using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TaskEntity"/> entity.
/// Sets primary key, property constraints, and relationships with User, TaskList, and Tag.
/// </summary>
public class TaskEntityConfiguration : BaseEntityConfiguration<TaskEntity>
{
    /// <summary>
    /// Configures the <see cref="TaskEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public override void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("tasks");

        builder.Property(t => t.Title)
            .HasConversion(t => t.Value, v => TaskTitle.Create(v))
            .HasColumnName("title")
            .HasMaxLength(TaskTitle.MaxLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasConversion(
                d => d != null ? d.Value : null,
                v => TaskDescription.CreateOptional(v))
            .HasColumnName("description")
            .HasMaxLength(TaskDescription.MaxLength);

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(t => t.OwnerId)
            .HasColumnName("owner_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(t => t.TaskListId)
            .HasColumnName("task_list_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(t => t.TagId)
            .HasColumnName("tag_id")
            .HasColumnType("uuid");

        builder.HasOne(t => t.Owner)
            .WithMany(u => u.OwnedTasks)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.TaskList)
            .WithMany(tl => tl.Tasks)
            .HasForeignKey(t => t.TaskListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Tag)
            .WithMany(t => t.Tasks)
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(t => t.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(t => t.UserAccesses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
