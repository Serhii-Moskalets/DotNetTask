using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="UserRepository"/> to verify its user-related queries.
/// Tests existence checks, retrieval by email and username.
/// </summary>
public class UserRepositoryTests
{
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByEmailAsync"/>
    /// returns true when a user with the specified email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByEmail_ReturnTrue_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsByEmailAsync("john@example.com");

        Assert.True(exists);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByUserNameAsync"/>
    /// returns true when a user with the specified username exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByUserName_ReturnTrue_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsByUserNameAsync("john");

        Assert.True(exists);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.ExistsByEmailAsync"/>
    /// performs a case-insensitive email comparison.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByEmail_IsCaseInsensitive()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "John@Example.com", this._passwordHash);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistsByEmailAsync("john@example.com"));
        Assert.True(await repo.ExistsByEmailAsync("JOHN@EXAMPLE.COM"));
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByEmailAsync"/>
    /// returns the correct user when the email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmail_ReturnUser_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var userEntity = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(userEntity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByEmailAsync(userEntity.Email.Value);

        Assert.NotNull(saved);
        Assert.Equal(userEntity.UserName, saved.UserName);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByEmailAsync"/>
    /// returns null when the email does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmail_ReturnNull_WhenUserDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var saved = await repo.GetByEmailAsync("john@example.com");

        Assert.Null(saved);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByUserNameAsync"/>
    /// returns the correct user when the username exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserName_ReturnUser_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var userEntity = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(userEntity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByUserNameAsync(userEntity.UserName.Value);

        Assert.NotNull(saved);
        Assert.Equal(userEntity.UserName, saved.UserName);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByUserNameAsync"/>
    /// returns null when the username does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserName_ReturnNull_WhenUserDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var saved = await repo.GetByUserNameAsync("user");

        Assert.Null(saved);
    }
}
