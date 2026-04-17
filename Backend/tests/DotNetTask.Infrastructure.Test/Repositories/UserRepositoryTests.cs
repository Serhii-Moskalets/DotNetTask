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
    private readonly DotNetTaskDbContext _context;
    private readonly UserRepository _repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepositoryTests"/> class.
    /// </summary>
    public UserRepositoryTests()
    {
        this._context = SqliteInMemoryDbContextFactory.Create();
        this._repo = new UserRepository(this._context);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.DeleteRangeAsync"/>
    /// removes multiple users from the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DeleteRange_Should_RemoveUsersFromDatabase()
    {
        // Arrange
        UserEntity user1 = UserEntityFactory.Create();
        UserEntity user2 = UserEntityFactory.Create(userName: "user2", email: "user2@example.com");
        UserEntity user3 = UserEntityFactory.Create(userName: "user3", email: "user3@example.com");

        await this._context.Users.AddRangeAsync(user1, user2, user3);
        await this._context.SaveChangesAsync();
        this._context.ChangeTracker.Clear();

        List<Guid> idsToDelete = [user1.Id, user2.Id];

        // Act
        await this._repo.DeleteRangeAsync(idsToDelete);

        // Assert
        List<UserEntity> remainingUsers = await this._context.Users.ToListAsync();
        remainingUsers.Should().HaveCount(1);
        remainingUsers.Should().Contain(u => u.Id == user3.Id);
        remainingUsers.Should().NotContain(u => idsToDelete.Contains(u.Id));
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.ExistsByEmailAsync"/>
    /// returns true when a user with the specified email exists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByEmail_Should_ReturnTrue_WhenUserExists()
    {
        // Arrange
        UserEntity testUser = await this.InitializeAsync();

        // Aсt
        bool exists = await this._repo.ExistsByEmailAsync(testUser.Email);

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
        // Act
        bool exists = await this._repo.ExistsByEmailAsync(Email.Create("ghost@example.com"));

        // Assert
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
        UserEntity testUser = await this.InitializeAsync();

        // Act
        bool exists = await this._repo.ExistsByUserNameAsync(testUser.UserName);

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
        // Act
        bool exists = await this._repo.ExistsByUserNameAsync(UserName.Create("ghost"));

        // Assert
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetPendingDeletionAsync"/>
    /// returns only users that have passed the deletion cutoff time.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetPendingDeletion_Should_ReturnOnlyUsersPastCutoff()
    {
        // Arrange
        DateTime cutoff = this.Clock.UtcNow;

        UserEntity userReady = UserEntityFactory.CreateActive(userName: "userReady", email: "userReady@example.com");
        userReady.RequestAccountDeletion(this.Clock.UtcNow.AddDays(-31));

        UserEntity userWaiting = UserEntityFactory.CreateActive(userName: "userWaiting", email: "userWaiting@example.com");
        userWaiting.RequestAccountDeletion(this.Clock.UtcNow);

        UserEntity userActive = UserEntityFactory.CreateActive(userName: "userActive", email: "userActive@example.com");

        await this._context.Users.AddRangeAsync(userReady, userWaiting, userActive);
        await this._context.SaveChangesAsync();

        // Act
        IReadOnlyList<Guid> result = await this._repo.GetPendingDeletionAsync(cutoff);

        // Arrange
        result.Should().NotBeNull();
        result.Should().Contain(userReady.Id);
        result.Should().NotContainInOrder(new List<Guid> { userActive.Id, userWaiting.Id });
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
        UserEntity testUser = await this.InitializeAsync();

        // Act
        UserEntity? saved = await this._repo.GetByEmailAsync(testUser.Email);

        // Assert
        saved.Should().NotBeNull();
        saved.UserName.Should().Be(testUser.UserName);
    }

    /// <summary>
    /// Checks that <see cref="UserRepository.GetByEmailAsync"/>
    /// returns null when the email does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByEmail_Should_ReturnNull_WhenUserDoesNotExist()
    {
        // Act
        UserEntity? saved = await this._repo.GetByEmailAsync(Email.Create("john@example.com"));

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
        UserEntity testUser = await this.InitializeAsync();

        // Act
        UserEntity? retrievedUser = await this._repo.GetByEmailAsync(testUser.Email, asNoTracking: false);

        // Assert
        bool isTracked = this._context.Entry(retrievedUser!).State != EntityState.Detached;
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
        UserEntity testUser = await this.InitializeAsync();

        // Act
        UserEntity? retrievedUser = await this._repo.GetByEmailAsync(testUser.Email, asNoTracking: true);

        // Assert
        bool isTracked = this._context.Entry(retrievedUser!).State != EntityState.Detached;
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
        UserEntity testUser = await this.InitializeAsync();

        // Act
        UserEntity? saved = await this._repo.GetByUserNameAsync(testUser.UserName);

        // Assert
        saved.Should().NotBeNull();
        saved.UserName.Should().Be(testUser.UserName);
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
        await this.InitializeAsync();

        // Act
        UserEntity? saved = await this._repo.GetByUserNameAsync(UserName.Create("user"));

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
        UserEntity testUser = await this.InitializeAsync();

        // Act
        (string SecurityStamp, bool MustChangePassword, UserStatus Status)? result = await this._repo.GetUsersSecurityInfoAsync(testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value.SecurityStamp.Should().Be(testUser.SecurityStamp.Value);
        result.Value.MustChangePassword.Should().Be(testUser.MustChangePassword);
    }

    /// <summary>
    /// Verifies that <see cref="UserRepository.GetUsersSecurityInfoAsync"/>
    /// returns null when the user does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsersSecurityInfo_Should_ReturnNull_WhenUserDoesNotExist()
    {
        // Act
        (string SecurityStamp, bool MustChangePassword, UserStatus Status)? result = await this._repo.GetUsersSecurityInfoAsync(Guid.NewGuid());

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
        UserEntity testUser = UserEntityFactory.CreateActive();
        testUser.RequestPasswordReset(token, TimeSpan.FromHours(1), this.Clock.UtcNow);

        await this.InitializeAsync(testUser);

        // Act
        UserEntity? result = await this._repo.GetBySecurityTokenAsync(token, UserTokenType.PasswordReset);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testUser.Id);
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
        UserEntity testUser = UserEntityFactory.CreateActive();
        testUser.RequestEmailChange(Email.Create("newemail@example.com"), "confirm-token", revertToken, TimeSpan.FromHours(1), this.Clock.UtcNow);

        await this.InitializeAsync(testUser);

        // Act
        UserEntity? result = await this._repo.GetBySecurityTokenAsync(revertToken, UserTokenType.EmailChangeRevert);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testUser.Id);
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
        UserEntity testUser = UserEntityFactory.CreateActive();
        testUser.RevertEmailChange(revertToken, this.Clock.UtcNow);

        await this.InitializeAsync(testUser);

        // Act
        UserEntity? result = await this._repo.GetBySecurityTokenAsync(revertToken, UserTokenType.PasswordReset);

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
        UserEntity testUser = UserEntityFactory.CreateActive();
        testUser.RequestPasswordReset(token, TimeSpan.FromHours(1), this.Clock.UtcNow);

        await this.InitializeAsync(testUser);

        // Act
        UserEntity? result = await this._repo.GetBySecurityTokenAsync(token, UserTokenType.PasswordReset);

        // Assert
        result.Should().NotBeNull();
        this._context.ChangeTracker.Entries<UserEntity>()
            .Should().ContainSingle(e => e.Entity == result);
    }

    private async Task<UserEntity> InitializeAsync(UserEntity? testUser = null)
    {
        UserEntity user = testUser ?? UserEntityFactory.CreateActive();
        await this._context.Users.AddAsync(user);
        await this._context.SaveChangesAsync();
        this._context.ChangeTracker.Clear();
        return user;
    }
}
