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
/// Provides unit tests for the <see cref="VerificationEmailResendEvent"/> class.
/// </summary>
public class VerificationEmailResendEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUrlProvider> _urlProviderMock;
    private readonly VerificationEmailResendEventHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerificationEmailResendEventHandlerTests"/> class.
    /// </summary>
    public VerificationEmailResendEventHandlerTests()
    {
        this._emailServiceMock = new Mock<IEmailService>();
        this._urlProviderMock = new Mock<IUrlProvider>();

        this._sut = new VerificationEmailResendEventHandler(this._emailServiceMock.Object, this._urlProviderMock.Object);
    }

    /// <summary>
    /// Verifies that when a <see cref="VerificationEmailResendEvent"/> is handled,
    /// the handler generates a confirmation link using <see cref="IUrlProvider"/>
    /// and sends an email via <see cref="IEmailService"/> with the correct data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test operation.</returns>
    [Fact]
    public async Task Handle_ShouldGeneratedLinkAndSendEmail_WhenEventIsRaised()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        SecurityToken token = SecurityToken.Create("test_token", TimeSpan.FromHours(1), UserTokenType.EmailVerification, CurrentTime);
        VerificationEmailResendEvent notification = new(user, token);
        string expectedLink = "https://test.com/confirm?token=test-token";

        this._urlProviderMock.Setup(x => x.GetEmailConfirmationLink(token.Value)).Returns(expectedLink);

        // Act
        await this._sut.Handle(notification, CancellationToken.None);

        // Assert
        this._urlProviderMock.Verify(
            x => x.GetEmailConfirmationLink(token.Value),
            Times.Once);

        this._emailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                user.Email.Value,
                user.UserName.Value,
                expectedLink,
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        UserEntity user = UserEntityFactory.Create();
        SecurityToken token = SecurityToken.Create("token", TimeSpan.FromHours(1), UserTokenType.EmailVerification, CurrentTime);
        VerificationEmailResendEvent notification = new(user, token);

        // Act
        await this._sut.Handle(notification, cts.Token);

        // Assert
        this._emailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cts.Token), Times.Once);
    }
}
