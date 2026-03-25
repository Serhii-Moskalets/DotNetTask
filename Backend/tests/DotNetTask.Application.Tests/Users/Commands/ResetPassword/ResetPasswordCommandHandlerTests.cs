using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.ResetPassword;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Users.Commands.ResetPassword;

/// <summary>
/// Contains unit tests for the <see cref="ResetPasswordCommandHandler"/> class.
/// </summary>
public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly ResetPasswordCommandHandler _sut;
    private readonly Mock<IClock> _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordCommandHandlerTests"/> class.
    /// </summary>
    public ResetPasswordCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._tokenGeneratorMock = new Mock<ITokenGenerator>();
        this._clock = new Mock<IClock>();

        this._sut = new ResetPasswordCommandHandler(
            this._unitOfWorkMock.Object,
            this._tokenGeneratorMock.Object,
            this._clock.Object);
    }

    /// <summary>
    /// Verifies that when a user exists, a token is generated,
    /// the user state is updated, and changes are saved.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_GenerateTokenAndSave_When_UserExists()
    {
        // Arrange
        ResetPasswordCommand command = new ResetPasswordCommand("existing@test.com");
        UserEntity user = UserEntityFactory.Create();
        const string secureToken = "secure-token-123";

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this._tokenGeneratorMock.Setup(x => x.GenerateSecureToken())
            .Returns(secureToken);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken!.Value.Should().Be(secureToken);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        this._tokenGeneratorMock.Verify(x => x.GenerateSecureToken(), Times.Once);
    }

    /// <summary>
    /// Verifies the security requirement: if the user does not exist,
    /// return success but do not perform any side effects.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccessButDoNothing_When_UserDoesNotExist()
    {
        // Arrange
        ResetPasswordCommand command = new ResetPasswordCommand("nonexistent@test.com");

        this._unitOfWorkMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<Email>(), asNoTracking: false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<bool> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        this._tokenGeneratorMock.Verify(x => x.GenerateSecureToken(), Times.Never);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
