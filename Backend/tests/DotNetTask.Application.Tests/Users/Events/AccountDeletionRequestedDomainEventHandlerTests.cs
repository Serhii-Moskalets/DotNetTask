using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Application.Users.Events;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Events;
using DotNetTask.Domain.Test.Common;
using Moq;

namespace DotNetTask.Application.Tests.Users.Events;

/// <summary>
/// Provides unit tests for the <see cref="AccountDeletionRequestedDomainEventHandler"/> class.
/// </summary>
public class AccountDeletionRequestedDomainEventHandlerTests : BaseTest
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AccountDeletionRequestedDomainEventHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDeletionRequestedDomainEventHandlerTests"/> class.
    /// </summary>
    public AccountDeletionRequestedDomainEventHandlerTests()
    {
        this._emailServiceMock = new Mock<IEmailService>();
        this._sut = new AccountDeletionRequestedDomainEventHandler(this._emailServiceMock.Object);
    }

    /// <summary>
    /// Verifies that when a <see cref="AccountDeletionRequestedDomainEvent"/> is handled,
    /// the handler sends an email via <see cref="IEmailService"/> with the correct data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test operation.</returns>
    [Fact]
    public async Task Handle_Should_CallEmailService_WithCorrectUserData()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        AccountDeletionRequestedDomainEvent notification = new(user);

        // Act
        await this._sut.Handle(notification, CancellationToken.None);

        // Assert
        this._emailServiceMock.Verify(
            x => x.SendAccountDeletedEmailAsync(
                user.Email.Value,
                user.UserName.Value,
                CancellationToken.None),
            Times.Once)
        ;
    }
}
