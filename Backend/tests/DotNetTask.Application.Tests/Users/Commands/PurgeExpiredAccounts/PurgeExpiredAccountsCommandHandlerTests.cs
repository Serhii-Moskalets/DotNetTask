using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.PurgeExpiredAccounts;
using DotNetTask.Domain.Test.Common;
using FluentAssertions;
using Moq;
using TinyResult;

namespace DotNetTask.Application.Tests.Users.Commands.PurgeExpiredAccounts;

/// <summary>
/// Provides unit tests for the <see cref="PurgeExpiredAccountsCommandHandler"/>
/// to ensure the maintenance task correctly identifies and removes expired accounts.
/// </summary>
public class PurgeExpiredAccountsCommandHandlerTests : BaseTest
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly PurgeExpiredAccountsCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PurgeExpiredAccountsCommandHandlerTests"/> class.
    /// </summary>
    public PurgeExpiredAccountsCommandHandlerTests()
    {
        this._uowMock.Setup(x => x.Users).Returns(this._repoMock.Object);
        this._sut = new PurgeExpiredAccountsCommandHandler(this._uowMock.Object, this.Clock);
    }

    /// <summary>
    /// Verifies that the handler returns a success result with zero deleted accounts
    /// when no users are scheduled for deletion before the current cutoff time.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnsZere_WhenNoExpiredUsers()
    {
        // Arrange
        this._repoMock
            .Setup(x => x.GetPendingDeletionAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        Result<int> result = await this._sut.Handle(new PurgeExpiredAccountsCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        this._repoMock.Verify(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that when expired user IDs are found, the handler invokes the bulk deletion
    /// and returns the correct count of purged accounts.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WhenExpiredUsersExist_DeletesThem()
    {
        // Arrange
        List<Guid> expiredIds = [Guid.NewGuid(), Guid.NewGuid()];

        this._repoMock
            .Setup(x => x.GetPendingDeletionAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredIds);

        this._uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredIds.Count);

        // Act
        Result<int> result = await this._sut.Handle(new PurgeExpiredAccountsCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        this._repoMock.Verify(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
