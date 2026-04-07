using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.ChangeEmail;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Users.Commands.ChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ChangeEmailCommandHandler"/> class.
/// </summary>
public class ChangeEmailCommandHandlerTests
{
    private const string NewEmail = "newemail@example.com";
    private const string GeneratedToken = "secure-token-123";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IClock> _clock;
    private readonly ChangeEmailCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeEmailCommandHandlerTests"/> class.
    /// </summary>
    public ChangeEmailCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._tokenGeneratorMock = new Mock<ITokenGenerator>();
        this._clock = new Mock<IClock>();
        this._sut = new ChangeEmailCommandHandler(
            this._unitOfWorkMock.Object,
            this._tokenGeneratorMock.Object,
            this._clock.Object);
    }

    /// <summary>
    /// Verifies that a valid email change request successfully updates the user state and saves changes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_RequestIsValid()
    {
        // Arrange
        ChangeEmailCommand command = new(NewEmail, Guid.NewGuid());
        UserEntity user = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken())
            .Returns(GeneratedToken);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken!.Value.Should().Be(GeneratedToken);
        user.CurrentToken.Metadata.Should().Be(command.NewEmail);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler returns a NotFound failure when the user is not found in the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        // Arrange
        ChangeEmailCommand command = new(NewEmail, Guid.NewGuid());

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);
        result.Error.Message.Should().Be(UserPolicy.AccountNotFoundMessage);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    /// <summary>
    /// Verifies that the handler returns an InvalidOperation failure when the new email is already taken.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailAlreadyInUse()
    {
        // Arrange
        ChangeEmailCommand command = new(NewEmail, Guid.NewGuid());
        UserEntity user = UserEntityFactory.Create();

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(x => x.Users.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.InvalidOperation);
        result.Error.Message.Should().Be(EmailPolicy.AlreadyInUseMessage);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        this._tokenGeneratorMock.Verify(x => x.GenerateSecureToken(), Times.Never());
    }

    /// <summary>
    /// Verifies that the handler returns a ValidationError when the new email is the same as the current email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_EmailIsSameAsCurrent()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        ChangeEmailCommand command = new(user.Email.Value, Guid.NewGuid());

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(command.UserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.ValidationError);
        result.Error.Message.Should().Be(EmailPolicy.SameAsCurrentMessage);

        this._unitOfWorkMock.Verify(x => x.Users.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()), Times.Never());

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
    }
}
