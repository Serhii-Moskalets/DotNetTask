using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmChangeEmail;
using TodoListApp.Application.Users.Commands.RevertEmailChange;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.RevertEmail;

/// <summary>
/// Unit tests for the <see cref="RevertEmailChangeCommandHandler"/> class.
/// </summary>
public class RevertEmailChangeCommandHandlerTests
{
    private const string PendingEmail = "new@example.com";
    private const string OldEmail = "old@example.com";
    private const string RevertToken = "revert-token";
    private const string ConfirmToken = "confirm-token";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RevertEmailChangeCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevertEmailChangeCommandHandlerTests"/> class.
    /// </summary>
    public RevertEmailChangeCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new RevertEmailChangeCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="RevertEmailChangeCommandHandler.Handle"/> successfully restores the original email address,
    /// clears security tokens, and persists changes when provided with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_DataIsValid()
    {
        // Arrange
        var user = CreateUser();
        var command = new RevertEmailChangeCommand(RevertToken);

        user.RequestEmailChange(Email.Create(PendingEmail), ConfirmToken, RevertToken, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailChangeRevert, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(OldEmail);
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
        user.RevertToken.Should().BeNull();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler propagates a <see cref="DomainException"/> and does not persist any changes
    /// if the provided revert token is incorrect or expired.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalid()
    {
        // Arrange
        var user = CreateUser();
        var command = new RevertEmailChangeCommand("wrong-token");

        user.RequestEmailChange(Email.Create(PendingEmail), ConfirmToken, RevertToken, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailChangeRevert, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => this._sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid or expired email change revert token.");

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
        var command = new RevertEmailChangeCommand("unknown-token");

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(command.Token, UserTokenType.EmailChangeRevert, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);
        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static UserEntity CreateUser()
        => new("John", "jonny", OldEmail, new('a', 64));
}
