using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Application.Users.Events;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Events;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

namespace DotNetTask.Application.Tests.Users.Events;

/// <summary>
/// Contains unit tests for the <see cref="PasswordResetRequestedDomainEventHandler"/> class.
/// </summary>
public class PasswordResetRequestedDomainEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUrlProvider> _urlProviderMock;
    private readonly PasswordResetRequestedDomainEventHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetRequestedDomainEventHandlerTests"/> class.
    /// </summary>
    public PasswordResetRequestedDomainEventHandlerTests()
    {
        this._emailServiceMock = new Mock<IEmailService>();
        this._urlProviderMock = new Mock<IUrlProvider>();
        this._sut = new PasswordResetRequestedDomainEventHandler(
            this._emailServiceMock.Object,
            this._urlProviderMock.Object);
    }

    /// <summary>
    /// Verifies that the handler generates a reset link and calls the email service with correct data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_SendEmail_When_EventIsRaised()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        SecurityToken token = SecurityToken.Create("reset-token", TimeSpan.FromHours(1), Domain.Enums.UserTokenType.PasswordReset, CurrentTime);
        PasswordResetRequestedDomainEvent notification = new(user, token);
        const string expectedLink = "https://todolist.com/reset-password?userId=...&token=...";

        this._urlProviderMock
            .Setup(x => x.GetPasswordResetLink(token.Value))
            .Returns(expectedLink);

        // Act
        await this._sut.Handle(notification, CancellationToken.None);

        // Assert
        this._emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                user.Email.Value,
                user.UserName.Value,
                expectedLink,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler throws an <see cref="ArgumentNullException"/> when the notification is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ThrowArgumentNullException_When_NotificationIsNull()
    {
        // Act
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
        SecurityToken token = SecurityToken.Create("reset-token", TimeSpan.FromHours(1), Domain.Enums.UserTokenType.PasswordReset, CurrentTime);
        PasswordResetRequestedDomainEvent notification = new(user, token);

        // Act
        await this._sut.Handle(notification, cts.Token);

        // Assert
        this._emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cts.Token), Times.Once);
    }
}
