using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Domain.Entities;
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
    /// Checks if a user with the specified email exists in the repository.
    /// </summary>
    /// <param name="email">The email of the user to check.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns <c>true</c> if a user with the email exists; otherwise, <c>false</c>.</returns>
    /// /// <remarks>
    /// Uses `EF.Functions.Like` for real databases, and case-insensitive comparison for InMemory provider.
    /// </remarks>
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (this.Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return await this.DbSet.AsNoTracking()
                .AnyAsync(
                    x => x.Email.Value.ToLowerInvariant() == email.ToLowerInvariant(),
                    cancellationToken);
        }

        return await this.DbSet.AsNoTracking()
            .AnyAsync(x => EF.Functions.Like(x.Email.Value, email), cancellationToken);
    }

    /// <summary>
    /// Checks if a user with the specified username exists in the repository.
    /// </summary>
    /// <param name="userName">The username of the user to check.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns <c>true</c> if a user with the username exists; otherwise, <c>false</c>.</returns>
    /// /// <remarks>
    /// Uses `EF.Functions.Like` for real databases, and case-insensitive comparison for InMemory provider.
    /// </remarks>
    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (this.Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return await this.DbSet.AsNoTracking()
                .AnyAsync(
                    x => x.UserName.Value == userName.ToLowerInvariant(),
                    cancellationToken);
        }

        return await this.DbSet.AsNoTracking()
            .AnyAsync(x => EF.Functions.Like(x.UserName.Value, userName), cancellationToken);
    }

    /// <summary>
    /// Retrieves a user entity by its email.
    /// </summary>
    /// <param name="email">The email of the user to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns the user entity if found; otherwise, <c>null</c>.</returns>
    /// /// <remarks>
    /// Uses `EF.Functions.Like` for real databases, and case-insensitive comparison for InMemory provider.
    /// </remarks>
    public async Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (this.Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return await this.DbSet.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Email.Value.ToLowerInvariant() == email.ToLowerInvariant(),
                    cancellationToken);
        }

        return await this.DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => EF.Functions.Like(x.Email.Value, email), cancellationToken);
    }

    /// <summary>
    /// Retrieves a user entity by its username.
    /// </summary>
    /// <param name="userName">The username of the user to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns the user entity if found; otherwise, <c>null</c>.</returns>
    /// /// <remarks>
    /// Uses `EF.Functions.Like` for real databases, and case-insensitive comparison for InMemory provider.
    /// </remarks>
    public async Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (this.Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return await this.DbSet.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserName.Value == userName.ToLowerInvariant(),
                    cancellationToken);
        }

        return await this.DbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => EF.Functions.Like(x.UserName.Value, userName), cancellationToken);
    }
}
