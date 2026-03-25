using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;

using TinyResult;

namespace DotNetTask.Infrastructure.Persistence.Repositories;

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
public class UserRepository(DotNetTaskDbContext context)
    : BaseRepository<UserEntity>(context), IUserRepository
{
    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email value object to check.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the email is taken; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await this.DbSet.AsNoTracking()
                .AnyAsync(x => x.Email == email, cancellationToken);
    }

    /// <summary>
    /// Checks if a user with the specified username exists.
    /// </summary>
    /// <param name="userName">The username value object to check.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the username is taken; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsByUserNameAsync(UserName userName, CancellationToken cancellationToken = default)
    {
        return await this.DbSet.AsNoTracking()
                    .AnyAsync(x => x.UserName == userName, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user entity by its email.
    /// </summary>
    /// <param name="email">The email value object.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<UserEntity?> GetByEmailAsync(Email email, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<UserEntity> query = this.DbSet.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    /// <summary>
    /// Retrieves a user entity that matches the specified security token and token type.
    /// </summary>
    /// <remarks>This method searches for a user entity whose current or revert token matches the provided
    /// token and token type. Ensure that the token and type correspond to a valid and active user token.</remarks>
    /// <param name="token">The security token used to identify the user. This value must not be null or empty.</param>
    /// <param name="tokenType">The type of the security token, which determines the context in which the token is valid.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user entity if a matching token
    /// and type are found; otherwise, null.</returns>
    public async Task<UserEntity?> GetBySecurityTokenAsync(string token, UserTokenType tokenType, CancellationToken cancellationToken = default)
    {
        return await this.DbSet.FirstOrDefaultAsync(
                x =>
                    (x.CurrentToken != null && x.CurrentToken.Value == token && x.CurrentToken.Type == tokenType) ||
                    (x.RevertToken != null && x.RevertToken.Value == token && x.RevertToken.Type == tokenType),
                cancellationToken);
    }

    /// <summary>
    /// Retrieves a user entity by its username.
    /// </summary>
    /// <param name="userName">The username value object.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<UserEntity?> GetByUserNameAsync(UserName userName, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<UserEntity> query = this.DbSet.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    /// <summary>
    /// Retrieves minimal security-related information for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A tuple with SecurityStamp and MustChangePassword, or null if user not found.</returns>
    /// <remarks>
    /// Optimized with projection to avoid fetching the entire entity.
    /// </remarks>
    public async Task<(string SecurityStamp, bool MustChangePassword, bool IsEmailConfirmed)?> GetUsersSecurityInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await this.DbSet
            .Where(u => u.Id == userId)
            .Select(u => new { u.SecurityStamp.Value, u.MustChangePassword, u.EmailConfirmed })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        return (user.Value, user.MustChangePassword, user.EmailConfirmed);
    }
}
