using Microsoft.EntityFrameworkCore;
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
        var user = this.CreateTestUser();

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
        var user = this.CreateTestUser();

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
        var userEntity = this.CreateTestUser();

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
    /// Verifies that the GetByEmailAsync method tracks the user entity in the context when the asNoTracking parameter
    /// is set to false.
    /// </summary>
    /// <remarks>This test ensures that the retrieved user entity is tracked by the Entity Framework Core
    /// context, confirming correct repository behavior when entity tracking is enabled.</remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetByEmail_ShouldTrackEntity_WhenAsNoTrackingIsFalse()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var email = Email.Create("track@test.com");
        var user = this.CreateTestUser(email: email.Value);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var retrievedUser = await repo.GetByEmailAsync(email, asNoTracking: false);

        // Assert
        var isTracked = context.Entry(retrievedUser!).State != EntityState.Detached;
        Assert.True(isTracked);
    }

    /// <summary>
    /// Verifies that retrieving a user by email with the 'asNoTracking' option set to <see langword="true"/> does not
    /// track the entity in the database context.
    /// </summary>
    /// <remarks>This test ensures that the repository's GetByEmailAsync method, when called with
    /// 'asNoTracking' set to <see langword="true"/>, returns an entity that is not tracked by the Entity Framework Core
    /// change tracker. This behavior is important for scenarios where tracking is unnecessary, as it can improve
    /// performance and reduce memory usage.</remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetByEmail_ShouldNotTrackEntity_WhenAsNoTrackingIsTrue()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var email = Email.Create("notrack@test.com");
        var user = this.CreateTestUser(email: email.Value);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var retrievedUser = await repo.GetByEmailAsync(email, asNoTracking: true);

        // Assert
        var isTracked = context.Entry(retrievedUser!).State != EntityState.Detached;
        Assert.False(isTracked);
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
        var userEntity = this.CreateTestUser();

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

        var user = this.CreateTestUser();

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

    private UserEntity CreateTestUser(string email = "test@example.com", string username = "testuser")
       => new("FirstName", username, email, this._passwordHash);
}
