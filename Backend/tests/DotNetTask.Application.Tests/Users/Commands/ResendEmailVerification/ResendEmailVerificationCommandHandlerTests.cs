using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Settings;
using DotNetTask.Application.Users.Commands.ResendEmailVerification;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.ResendEmailVerification;

/// <summary>
/// Contains unit tests for the <see cref="ResendEmailVerificationCommandHandler"/> class.
/// </summary>
public class ResendEmailVerificationCommandHandlerTests
{
    private const string IpAddress = "192.168.0.1";
    private const string GeneratedToken = "secure-test-token";
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly TokenOptions _tokenSettings;
    private readonly ResendEmailVerificationCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResendEmailVerificationCommandHandlerTests"/> class.
    /// </summary>
    public ResendEmailVerificationCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._clockMock = new Mock<IClock>();
        this._tokenGeneratorMock = new Mock<ITokenGenerator>();
        this._tokenSettings = new TokenOptions
        {
            EmailVerificationTokenDuration = TimeSpan.FromHours(24),
        };

        this._handler = new ResendEmailVerificationCommandHandler(
            this._uowMock.Object,
            this._tokenGeneratorMock.Object,
            this._tokenSettings,
            this._clockMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a NotFound error code
    /// when the requested user does not exist in the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserDoesNotExist()
    {
        // Arrange
        ResendEmailVerificationCommand command = new(Guid.NewGuid(), IpAddress);
        this._uowMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<Guid>(), false, default))
            .ReturnsAsync((UserEntity)null!);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        this._uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result with a ValidationError
    /// when the user's email has already been confirmed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailAlreadyConfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        ResendEmailVerificationCommand command = new(user.Id, IpAddress);

        user.RequestEmailVerification(GeneratedToken, TimeSpan.FromHours(1), CurrentTime);
        user.ConfirmEmailVerification(GeneratedToken, CurrentTime.AddMinutes(15));

        this._uowMock.Setup(u => u.Users.GetByIdAsync(user.Id, false, default))
            .ReturnsAsync(user);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
        this._uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler successfully generates a new secure token,
    /// assigns it to the user, and persists the changes when the user's email is unconfirmed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_GenerateNewTokenAndSave_WhenUserIsUnconfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        ResendEmailVerificationCommand command = new(user.Id, IpAddress);

        this._uowMock.Setup(u => u.Users.GetByIdAsync(user.Id, false, default))
            .ReturnsAsync(user);

        this._tokenGeneratorMock.Setup(t => t.GenerateSecureToken())
            .Returns(GeneratedToken);

        this._clockMock.Setup(c => c.UtcNow).Returns(CurrentTime);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.CurrentToken!.Value.Should().Be(GeneratedToken);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
