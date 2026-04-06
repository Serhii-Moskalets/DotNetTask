using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Application.Users.Events;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Events;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using Moq;

namespace DotNetTask.Application.Tests.Users.Events;

/// <summary>
/// Provides unit tests for the <see cref="UserRegisteredDomainEventHandler"/> class.
/// </summary>
public class UserRegisteredDomainEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUrlProvider> _urlProviderMock;
    private readonly UserRegisteredDomainEventHandler _sut;

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
        UserEntity user = UserEntityFactory.Create();
        SecurityToken token = SecurityToken.Create("test-token", TimeSpan.FromDays(1), UserTokenType.EmailVerification, CurrentTime);
        UserRegisteredDomainEvent notification = new(user, token);
        string expectedLink = "https://test.com/confirm?token=test-token";

        this._urlProviderMock
            .Setup(x => x.GetEmailConfirmationLink(token.Value))
            .Returns(expectedLink);

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
}
