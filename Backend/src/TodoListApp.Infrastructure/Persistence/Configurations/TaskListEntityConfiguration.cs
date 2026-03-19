using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="TaskListEntity"/> entity.
/// Sets primary key, property constraints, and relationship with User.
/// </summary>
public class TaskListEntityConfiguration : BaseEntityConfiguration<TaskListEntity>
{
    /// <summary>
    /// Configures the <see cref="TaskListEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public override void Configure(EntityTypeBuilder<TaskListEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("task_lists");

        builder.Property(tl => tl.OwnerId)
            .HasColumnName("owner_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(tl => tl.Title)
            .HasConversion(t => t.Value, v => TaskListTitle.Create(v))
            .HasColumnName("title")
            .HasMaxLength(TaskListTitle.MaxLength)
            .IsRequired();

        builder.HasOne(tl => tl.Owner)
            .WithMany(o => o.TaskLists)
            .HasForeignKey(tl => tl.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(tl => tl.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
