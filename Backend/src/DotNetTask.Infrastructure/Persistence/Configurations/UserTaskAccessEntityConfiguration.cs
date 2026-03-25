using DotNetTask.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="UserTaskAccessEntity"/> entity.
/// Sets composite primary key and relationships with <see cref="UserEntity"/> and <see cref="TaskEntity"/>.
/// </summary>
public class UserTaskAccessEntityConfiguration : IEntityTypeConfiguration<UserTaskAccessEntity>
{
    /// <summary>
    /// Configures the <see cref="UserTaskAccessEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserTaskAccessEntity> builder)
    {
        builder.ToTable("user_task_access");

        builder.HasKey(uta => new { uta.UserId, uta.TaskId });

        builder.Property(uta => uta.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        builder.Property(uta => uta.TaskId)
            .HasColumnName("task_id")
            .HasColumnType("uuid");

        builder.Property(uta => uta.CreatedDate)
            .HasColumnName("created_date");

        builder.HasOne(uta => uta.User)
            .WithMany(u => u.UserAccesses)
            .HasForeignKey(uta => uta.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uta => uta.Task)
            .WithMany(t => t.UserAccesses)
            .HasForeignKey(uta => uta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(uta => uta.TaskId);
    }
}
