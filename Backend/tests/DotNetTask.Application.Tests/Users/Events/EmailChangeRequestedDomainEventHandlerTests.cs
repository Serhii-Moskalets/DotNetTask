using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Application.Users.Events;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Events;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

namespace DotNetTask.Application.Tests.Users.Events;

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
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

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
        string newEmail = "new@email.com";
        string oldEmail = "old@email.com";

        UserEntity user = UserEntityFactory.Create(email: oldEmail);

        SecurityToken confirmToken = SecurityToken.Create("confirm-123", TimeSpan.FromHours(1), UserTokenType.EmailChange, CurrentTime, newEmail);
        SecurityToken revertToken = SecurityToken.Create("revert-123", TimeSpan.FromHours(1), UserTokenType.EmailChangeRevert, CurrentTime, oldEmail);

        EmailChangeRequestedDomainEvent domainEvent = new(user, confirmToken, revertToken);

        this._urlProviderMock.Setup(x => x.GetEmailChangeLink(It.IsAny<string>()))
            .Returns("http://confirm-link.com");
        this._urlProviderMock.Setup(x => x.GetEmailRevertLink(It.IsAny<string>()))
            .Returns("http://revert-link.com");

        // Act
        await this._sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        confirmToken.Metadata.Should().NotBeNull();
        revertToken.Metadata.Should().NotBeNull();

        this._emailServiceMock.Verify(
            x => x.SendEmailChangeConfirmationAsync(
                confirmToken.Metadata,
                user.UserName.Value,
                "http://confirm-link.com",
                It.IsAny<CancellationToken>()), Times.Once);

        this._emailServiceMock.Verify(
            x => x.SendEmailChangeSecurityAlertAsync(
                revertToken.Metadata,
                confirmToken.Metadata,
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
        UserEntity user = UserEntityFactory.Create();
        SecurityToken confirmToken = SecurityToken.Create("confirm-123", TimeSpan.FromHours(1), UserTokenType.EmailChange, CurrentTime);
        SecurityToken revertToken = SecurityToken.Create("revert-123", TimeSpan.FromHours(1), UserTokenType.EmailChangeRevert, CurrentTime, metadata: null);

        EmailChangeRequestedDomainEvent domainEvent = new(user, confirmToken, revertToken);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this._sut.Handle(domainEvent, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that the handler throws an <see cref="ArgumentNullException"/>
    /// when the notification event is <see langword="null"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test operation.</returns>
    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenNotificationIsNull()
    {
        // Act & Assert
        Func<Task> act = () => this._sut.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the <see cref="CancellationToken"/> provided to the handler
    /// is correctly propagated to the <see cref="IEmailService"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test operation.</returns>
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToEmailService()
    {
        // Arrange
        CancellationTokenSource cts = new();
        string newEmail = "new@email.com";
        string oldEmail = "old@email.com";

        UserEntity user = UserEntityFactory.Create(email: oldEmail);

        SecurityToken confirmToken = SecurityToken.Create("confirm-123", TimeSpan.FromHours(1), UserTokenType.EmailChange, CurrentTime, newEmail);
        SecurityToken revertToken = SecurityToken.Create("revert-123", TimeSpan.FromHours(1), UserTokenType.EmailChangeRevert, CurrentTime, oldEmail);

        EmailChangeRequestedDomainEvent domainEvent = new(user, confirmToken, revertToken);

        // Act
        await this._sut.Handle(domainEvent, cts.Token);

        // Assert
        this._emailServiceMock.Verify(
            x => x.SendEmailChangeSecurityAlertAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cts.Token), Times.Once);
    }
}
