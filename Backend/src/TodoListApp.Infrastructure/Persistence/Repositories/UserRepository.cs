using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="UserEntity"/> instances.
/// Inherits from <see cref="BaseRepository{TEntity}"/> to provide basic CRUD operations,
/// and implements <see cref="IUserRepository"/> to define additional user-specific queries.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserRepository"/> class
/// with the specified database context.
/// </remarks>
/// <param name="context">The database context used for data operations.</param>
public class UserRepository(TodoListAppDbContext context)
    : BaseRepository<UserEntity>(context), IUserRepository
{
    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email value object to check.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the email is taken; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
            .AnyAsync(x => x.Email == email, cancellationToken);

    /// <summary>
    /// Checks if a user with the specified username exists.
    /// </summary>
    /// <param name="userName">The username value object to check.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the username is taken; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsByUserNameAsync(UserName userName, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
                .AnyAsync(x => x.UserName == userName, cancellationToken);

    /// <summary>
    /// Retrieves a user entity by its email.
    /// </summary>
    /// <param name="email">The email value object.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<UserEntity?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    /// <summary>
    /// Retrieves a user entity by its username.
    /// </summary>
    /// <param name="userName">The username value object.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<UserEntity?> GetByUserNameAsync(UserName userName, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);

    /// <summary>
    /// Retrieves minimal security-related information for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A tuple with SecurityStamp and MustChangePassword, or null if user not found.</returns>
    /// <remarks>
    /// Optimized with projection to avoid fetching the entire entity.
    /// </remarks>
    public async Task<(string SecurityStamp, bool MustChangePassword)?> GetUsersSecurityInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await this.DbSet
            .Where(u => u.Id == userId)
            .Select(u => new { u.SecurityStamp.Value, u.MustChangePassword })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        return (user.Value, user.MustChangePassword);
    }
}
