using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Application.Application.Users.Events;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Events;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Events;

/// <summary>
/// Provides unit tests for the <see cref="UserRegisteredDomainEventHandler"/> class.
/// </summary>
public class UserRegisteredDomainEventHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUrlProvider> _urlProviderMock;
    private readonly UserRegisteredDomainEventHandler _sut;
    private readonly string _passwordHash = new('a', 60);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRegisteredDomainEventHandlerTests"/> class
    /// and mocks the required dependencies.
    /// </summary>
    public UserRegisteredDomainEventHandlerTests()
    {
        this._emailServiceMock = new Mock<IEmailService>();
        this._urlProviderMock = new Mock<IUrlProvider>();
        this._sut = new UserRegisteredDomainEventHandler(this._emailServiceMock.Object, this._urlProviderMock.Object);
    }

    /// <summary>
    /// Verifies that when a <see cref="UserRegisteredDomainEvent"/> is handled,
    /// the handler generates a confirmation link using <see cref="IUrlProvider"/>
    /// and sends an email via <see cref="IEmailService"/> with the correct data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test operation.</returns>
    [Fact]
    public async Task Handle_ShouldGenerateLinkAndSendEmail_WhenEventIsRaised()
    {
        // Arrange
        var user = new UserEntity(
            "John",
            "john_doe",
            "john@example.com",
            this._passwordHash);
        var token = SecurityToken.Create("test-token", TimeSpan.FromDays(1), UserTokenType.EmailVerification);
        var notification = new UserRegisteredDomainEvent(user, token);
        var expectedLink = "https://test.com/confirm?token=test-token";

        this._urlProviderMock
            .Setup(x => x.GetEmailConfirmationLink(user.Email.Value, token.Value))
            .Returns(expectedLink);

        // Act
        await this._sut.Handle(notification, CancellationToken.None);

        // Assert
        this._urlProviderMock.Verify(
            x => x.GetEmailConfirmationLink(user.Email.Value, token.Value),
            Times.Once);

        this._emailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                user.Email.Value,
                user.UserName.Value,
                expectedLink,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
