using Moq;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();

        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Aсt
        var exists = await repo.ExistsByEmailAsync(user.Email);

        // Assert
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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();

        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var exists = await repo.ExistsByUserNameAsync(user.UserName);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByEmailAsync"/>
    /// returns the correct user when the email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmail_ReturnUser_WhenUserExists()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();

        var repo = new UserRepository(context);
        var userEntity = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        await context.Users.AddAsync(userEntity);
        await context.SaveChangesAsync();

        // Act
        var saved = await repo.GetByEmailAsync(userEntity.Email);

        // Assert
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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);

        // Act
        var saved = await repo.GetByEmailAsync(Email.Create("john@example.com"));

        // Assert
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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();

        var repo = new UserRepository(context);
        var userEntity = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        await context.Users.AddAsync(userEntity);
        await context.SaveChangesAsync();

        // Act
        var saved = await repo.GetByUserNameAsync(userEntity.UserName);

        // Assert
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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);

        // Act
        var saved = await repo.GetByUserNameAsync(UserName.Create("user"));

        // Assert
        Assert.Null(saved);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetUsersSecurityInfoAsync"/>
    /// returns correct security data when the user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsersSecurityInfo_ReturnData_WhenUserExists()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);

        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetUsersSecurityInfoAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.SecurityStamp.Value, result.Value.SecurityStamp);
        Assert.Equal(user.MustChangePassword, result.Value.MustChangePassword);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetUsersSecurityInfoAsync"/>
    /// returns null when the user does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsersSecurityInfo_ReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);

        // Act
        var result = await repo.GetUsersSecurityInfoAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
