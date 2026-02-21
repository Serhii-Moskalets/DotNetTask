using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Application.Users.Events;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Events;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Events;

/// <summary>
/// Unit tests for the <see cref="EmailChangeRequestedDomainEventHandler"/> class.
/// </summary>
/// <remarks>
/// These tests ensure that the domain event handler correctly coordinates
/// between the URL provider and the email service to notify users about
/// pending email changes and security alerts.
/// </remarks>
public class EmailChangeRequestedDomainEventHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUrlProvider> _urlProviderMock;
    private readonly EmailChangeRequestedDomainEventHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailChangeRequestedDomainEventHandlerTests"/> class.
    /// </summary>
    public EmailChangeRequestedDomainEventHandlerTests()
    {
        this._emailServiceMock = new Mock<IEmailService>();
        this._urlProviderMock = new Mock<IUrlProvider>();
        this._sut = new EmailChangeRequestedDomainEventHandler(
            this._emailServiceMock.Object,
            this._urlProviderMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="EmailChangeRequestedDomainEventHandler.Handle"/> successfully
    /// triggers both the confirmation email and the security alert email when the event data is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldSendBothEmails_WhenDataIsValid()
    {
        // Arrange
        var user = CreateTestUser();
        var confirmToken = SecurityToken.Create("confirm-123", TimeSpan.FromHours(1), UserTokenType.EmailChange);
        var revertToken = SecurityToken.Create("revert-123", TimeSpan.FromHours(1), UserTokenType.EmailChangeRevert, "old@email.com");

        var domainEvent = new EmailChangeRequestedDomainEvent(user, confirmToken, revertToken);

        this._urlProviderMock.Setup(x => x.GetEmailChangeLink(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("http://confirm-link.com");
        this._urlProviderMock.Setup(x => x.GetEmailRevertLink(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns("http://revert-link.com");

        // Act
        await this._sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        this._emailServiceMock.Verify(
            x => x.SendEmailChangeConfirmationAsync(
                user.Email.Value,
                user.UserName.Value,
                "http://confirm-link.com",
                It.IsAny<CancellationToken>()), Times.Once);

        this._emailServiceMock.Verify(
            x => x.SendEmailChangeSecurityAlertAsync(
                "old@email.com",
                user.Email.Value,
                user.UserName.Value,
                "http://revert-link.com",
                It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="EmailChangeRequestedDomainEventHandler.Handle"/> throws an
    /// <see cref="ArgumentNullException"/> when the revert token's metadata (original email) is missing.
    /// </summary>
    /// <remarks>
    /// This scenario ensures that the system fails fast if critical security information
    /// required for the revert process is not present.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldThrowException_WhenOriginalEmailIsMissingInMetadata()
    {
        // Arrange
        var user = CreateTestUser();
        var confirmToken = SecurityToken.Create("confirm-123", TimeSpan.FromHours(1), UserTokenType.EmailChange);
        var revertToken = SecurityToken.Create("revert-123", TimeSpan.FromHours(1), UserTokenType.EmailChangeRevert, metadata: null);

        var domainEvent = new EmailChangeRequestedDomainEvent(user, confirmToken, revertToken);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this._sut.Handle(domainEvent, CancellationToken.None));
    }

    private static UserEntity CreateTestUser()
        => new("John", "Smith", "new@email.com", new('a', 64));
}
