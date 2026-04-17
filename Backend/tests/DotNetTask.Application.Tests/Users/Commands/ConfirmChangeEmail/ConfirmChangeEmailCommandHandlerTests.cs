using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Users.Commands.ConfirmEmailChange;
using DotNetTask.Domain.Common;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmEmailChangeCommandHandler"/> class.
/// </summary>
public class ConfirmEmailChangeCommandHandlerTests : BaseTest
{
    private const string PendingEmail = "new@example.com";
    private const string ConfirmToken = "confirm-token";
    private const string RevertToken = "revert-token";
    private const string IpAddress = "192.168.0.1";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfirmEmailChangeCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailChangeCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmEmailChangeCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new ConfirmEmailChangeCommandHandler(this._unitOfWorkMock.Object, this.Clock);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the user is not found by the token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFail_When_UserNotFound()
    {
        // Arrange
        ConfirmEmailChangeCommand command = new("non-existent-token", IpAddress);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
            It.IsAny<string>(),
            UserTokenType.EmailChange,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler throws a DomainException when the user is found,
    /// but the token is invalid or expired (Domain Logic check).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_TokenIsInvalidForFoundUser()
    {
        // Arrange
        ConfirmEmailChangeCommand command = new("wrong-token", IpAddress);
        UserEntity user = UserEntityFactory.Create();

        user.RequestEmailChange(Email.Create(PendingEmail), ConfirmToken, RevertToken, TimeSpan.FromHours(1), this.Clock.UtcNow);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
                It.IsAny<string>(),
                UserTokenType.EmailChange,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the handler returns a success result, updates the user's email
    /// to the pending one, and clears the security token when a valid and
    /// non-expired token is provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        ConfirmEmailChangeCommand command = new(ConfirmToken, IpAddress);
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestEmailChange(Email.Create(PendingEmail), ConfirmToken, RevertToken, TimeSpan.FromHours(1), this.Clock.UtcNow);

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(ConfirmToken, UserTokenType.EmailChange, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Result<Unit> result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(PendingEmail);
        user.CurrentToken.Should().BeNull();
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
