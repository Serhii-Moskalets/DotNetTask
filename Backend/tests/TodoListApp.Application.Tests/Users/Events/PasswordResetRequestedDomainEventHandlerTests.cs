using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Application.Users.Events;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Events;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Events;

/// <summary>
/// Contains unit tests for the <see cref="PasswordResetRequestedDomainEventHandler"/> class.
/// </summary>
public class PasswordResetRequestedDomainEventHandlerTests
{
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
        var user = new UserEntity(
            "JohnDoe",
            "john_doe",
            "john@example.com",
            new string('a', 64));

        var token = SecurityToken.Create("reset-token", TimeSpan.FromHours(1), Domain.Enums.UserTokenType.PasswordReset);
        var notification = new PasswordResetRequestedDomainEvent(user, token);
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
}
