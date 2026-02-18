using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="UserEntity"/> entity.
/// Sets primary key, property constraints, and unique indexes.
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <summary>
    /// Configures the <see cref="UserEntity"/> entity type.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(u => u.FirstName)
            .HasConversion(n => n.Value, v => FirstName.Create(v))
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.LastName)
            .HasConversion(
                n => n != null ? n.Value : null,
                v => LastName.Create(v))
            .HasColumnName("last_name")
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(u => u.UserName)
            .HasConversion(un => un.Value, v => UserName.Create(v))
            .HasColumnName("user_name")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.EmailConfirmed)
            .HasColumnName("email_confirmed")
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasConversion(ph => ph.Value, v => PasswordHash.Create(v))
            .HasColumnName("password_hash")
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

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
