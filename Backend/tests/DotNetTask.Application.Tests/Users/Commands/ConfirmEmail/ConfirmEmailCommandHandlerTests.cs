using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.ConfirmEmail;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Test.Common;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.ConfirmEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmEmailCommandHandler"/> class.
/// </summary>
public class ConfirmEmailCommandHandlerTests
{
    private const string IpAddress = "192.168.0.1";
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IClock> _clock;
    private readonly ConfirmEmailCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmEmailCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._clock = new Mock<IClock>();
        this._sut = new ConfirmEmailCommandHandler(this._unitOfWorkMock.Object, this._clock.Object);
    }

    /// <summary>
    /// Verifies that a valid token successfully confirms the email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        const string token = "valid-token";
        ConfirmEmailCommand command = new(token, IpAddress);

        UserEntity user = UserEntityFactory.Create();

        user.RequestEmailVerification(token, TimeSpan.FromHours(1), CurrentTime);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that a DomainException is thrown when the token is invalid or expired.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalid()
    {
        // Arrange
        ConfirmEmailCommand command = new("wrong-token", IpAddress);
        UserEntity user = UserEntityFactory.Create();

        user.RequestEmailVerification("valid-token", TimeSpan.FromHours(1), CurrentTime);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a NotFound error when attempting to revert an email change with a non-existent
    /// security token.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_TokenDoesNotExist()
    {
        // Arrange
        ConfirmEmailCommand command = new("unknown-token", IpAddress);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
