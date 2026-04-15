using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetTask.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="UserEntity"/> entity.
/// Sets primary key, property constraints, and unique indexes.
/// </summary>
public class UserEntityConfiguration : BaseEntityConfiguration<UserEntity>
{
    /// <summary>
    /// Configures the <see cref="UserEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public override void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("users");

        builder.Property(u => u.FirstName)
            .HasConversion(n => n.Value, v => FirstName.Create(v))
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(FirstName.MaxLength);

        builder.Property(u => u.LastName)
            .HasConversion(
                n => n != null ? n.Value : null,
                v => LastName.CreateOptional(v))
            .HasColumnName("last_name")
            .HasMaxLength(LastName.MaxLength)
            .IsRequired(false);

        builder.Property(u => u.UserName)
            .HasConversion(un => un.Value, v => UserName.Create(v))
            .HasColumnName("user_name")
            .IsRequired()
            .HasMaxLength(UserName.MaxLength);

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(Email.MaxLength);

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasConversion(ph => ph.Value, v => PasswordHash.Create(v))
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.SecurityStamp)
            .HasConversion(s => s.Value, v => SecurityStamp.Create(v))
            .HasColumnName("security_stamp")
            .IsRequired();

        builder.Property(u => u.MustChangePassword)
            .HasColumnName("must_change_password")
            .HasDefaultValue(false)
            .IsRequired();

        builder.OwnsOne(u => u.CurrentToken, token =>
        {
            token.WithOwner();

            token.Property(t => t.Value)
                .HasColumnName("token_value")
                .HasMaxLength(255);

            token.Property(t => t.ExpiresAt)
                .HasColumnName("token_expires_at");

            token.Property(t => t.Type)
                .HasColumnName("token_type")
                .HasConversion<string>();

            token.Property(t => t.Metadata)
                .HasColumnName("token_metadata")
                .HasMaxLength(100);
        });

        builder.Navigation(u => u.CurrentToken).IsRequired(false);

        builder.OwnsOne(u => u.RevertToken, token =>
        {
            token.WithOwner();

            token.Property(t => t.Value)
                .HasColumnName("revert_token_value")
                .HasMaxLength(255);

            token.Property(t => t.ExpiresAt)
                .HasColumnName("revert_token_expires_at");

            token.Property(t => t.Type)
                .HasColumnName("revert_token_type")
                .HasConversion<string>();

            token.Property(t => t.Metadata)
                .HasColumnName("revert_token_metadata")
                .HasMaxLength(100);
        });

        builder.Navigation(u => u.RevertToken).IsRequired(false);

        builder.Navigation(u => u.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.TaskLists)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.OwnedTasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.UserAccesses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
