using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="UserRepository"/> to verify its user-related queries.
/// Tests existence checks, retrieval by email and username.
/// </summary>
public class UserRepositoryTests : BaseTest
{
    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByEmailAsync"/>
    /// returns true when a user with the specified email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByEmail_Should_ReturnTrue_WhenUserExists()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Aсt
        bool exists = await repo.ExistsByEmailAsync(user.Email);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByEmailAsync"/>
    /// returns false when no user with the specified email exists in the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByEmail_Should_ReturnFalse_WhenUserDoesNotExist()
    {
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserRepository repo = new(context);

        bool exists = await repo.ExistsByEmailAsync(Email.Create("ghost@example.com"));

        exists.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByUserNameAsync"/>
    /// returns true when a user with the specified username exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByUserName_Should_ReturnTrue_WhenUserExists()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        bool exists = await repo.ExistsByUserNameAsync(user.UserName);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByUserNameAsync"/>
    /// returns false when no user with the specified username exists in the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByUserName_Should_ReturnFalse_WhenUserDoesNotExist()
    {
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserRepository repo = new(context);

        bool exists = await repo.ExistsByUserNameAsync(UserName.Create("ghost"));

        exists.Should().BeFalse();
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByEmailAsync"/>
    /// returns the correct user when the email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmail_Should_ReturnUser_WhenUserExists()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        UserEntity? saved = await repo.GetByEmailAsync(user.Email);

        // Assert
        saved.Should().NotBeNull();
        saved.UserName.Should().Be(user.UserName);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByEmailAsync"/>
    /// returns null when the email does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmail_Should_ReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserRepository repo = new(context);

        // Act
        UserEntity? saved = await repo.GetByEmailAsync(Email.Create("john@example.com"));

        // Assert
        saved.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the GetByEmailAsync method tracks the user entity in the context when the asNoTracking parameter
    /// is set to false.
    /// </summary>
    /// <remarks>This test ensures that the retrieved user entity is tracked by the Entity Framework Core
    /// context, confirming correct repository behavior when entity tracking is enabled.</remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetByEmail_Should_TrackEntity_WhenAsNoTrackingIsFalse()
    {
        // Arrange
        Email email = Email.Create("track@test.com");
        UserEntity user = UserEntityFactory.Create(email: email.Value);

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        UserEntity? retrievedUser = await repo.GetByEmailAsync(email, asNoTracking: false);

        // Assert
        bool isTracked = context.Entry(retrievedUser!).State != EntityState.Detached;
        isTracked.Should().BeTrue();
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
    public async Task GetByEmail_Should_NotTrackEntity_WhenAsNoTrackingIsTrue()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserRepository repo = new(context);
        Email email = Email.Create("notrack@test.com");
        UserEntity user = UserEntityFactory.Create(email: email.Value);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        UserEntity? retrievedUser = await repo.GetByEmailAsync(email, asNoTracking: true);

        // Assert
        bool isTracked = context.Entry(retrievedUser!).State != EntityState.Detached;
        isTracked.Should().BeFalse();
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByUserNameAsync"/>
    /// returns the correct user when the username exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserName_Should_ReturnUser_WhenUserExists()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        UserEntity? saved = await repo.GetByUserNameAsync(user.UserName);

        // Assert
        saved.Should().NotBeNull();
        saved.UserName.Should().Be(user.UserName);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByUserNameAsync"/>
    /// returns null when the username does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserName_Should_ReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserRepository repo = new(context);

        // Act
        UserEntity? saved = await repo.GetByUserNameAsync(UserName.Create("user"));

        // Assert
        saved.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetUsersSecurityInfoAsync"/>
    /// returns correct security data when the user exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsersSecurityInfo_Should_ReturnData_WhenUserExists()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        (string SecurityStamp, bool MustChangePassword, UserStatus Status)? result = await repo.GetUsersSecurityInfoAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value.SecurityStamp.Should().Be(user.SecurityStamp.Value);
        result.Value.MustChangePassword.Should().Be(user.MustChangePassword);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetUsersSecurityInfoAsync"/>
    /// returns null when the user does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsersSecurityInfo_Should_ReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserRepository repo = new(context);

        // Act
        (string SecurityStamp, bool MustChangePassword, UserStatus Status)? result = await repo.GetUsersSecurityInfoAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetBySecurityTokenAsync"/>
    /// returns the user when the string token and type match the CurrentToken.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetBySecurityToken_Should_ReturnUser_WhenMatchesCurrentToken()
    {
        // Arrange
        string token = "current-secret";

        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestPasswordReset(token, TimeSpan.FromHours(1), this.Clock.UtcNow);

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        UserEntity? result = await repo.GetBySecurityTokenAsync(token, UserTokenType.PasswordReset);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetBySecurityTokenAsync"/>
    /// returns the user when the string token and type match the RevertToken.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetBySecurityToken_Should_ReturnUser_WhenMatchesRevertToken()
    {
        // Arrange
        string revertToken = "revert-token";

        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestEmailChange(Email.Create("newemail@example.com"), "confirm-token", revertToken, TimeSpan.FromHours(1), this.Clock.UtcNow);

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        UserEntity? result = await repo.GetBySecurityTokenAsync(revertToken, UserTokenType.EmailChangeRevert);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetBySecurityTokenAsync"/>
    /// returns null when the token value matches but the token type is different.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetBySecurityToken_Should_ReturnNull_WhenTypeMismatch()
    {
        // Arrange
        string revertToken = "revert-token";

        UserEntity user = UserEntityFactory.Create();
        user.RevertEmailChange(revertToken, this.Clock.UtcNow);

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        UserEntity? result = await repo.GetBySecurityTokenAsync(revertToken, UserTokenType.PasswordReset);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetBySecurityTokenAsync"/>
    /// returns a tracked entity so it can be updated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetBySecurityToken_Should_ShouldTrackReturnedEntity()
    {
        // Arrange
        string token = "track-token";

        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestPasswordReset(token, TimeSpan.FromHours(1), this.Clock.UtcNow);

        DotNetTaskDbContext context = await CreateContextWithUser(user);
        UserRepository repo = new(context);

        // Act
        UserEntity? result = await repo.GetBySecurityTokenAsync(token, UserTokenType.PasswordReset);

        // Assert
        result.Should().NotBeNull();
        context.ChangeTracker.Entries<UserEntity>()
            .Should().ContainSingle(e => e.Entity == result);
    }

    private static async Task<DotNetTaskDbContext> CreateContextWithUser(UserEntity user)
    {
        DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return context;
    }
}
