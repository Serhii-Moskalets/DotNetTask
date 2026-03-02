using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmChangeEmail;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmChangeEmailCommandHandler"/> class.
/// </summary>
public class ConfirmChangeEmailCommandHandlerTests
{
    private const string PendingEmail = "new@example.com";
    private const string ConfirmToken = "confirm-token";
    private const string RevertToken = "revert-token";
    private readonly string _passwordHash = new('a', 64);

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfirmChangeEmailCommandHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmChangeEmailCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmChangeEmailCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new ConfirmChangeEmailCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the user is not found by the token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnFail_When_UserNotFound()
    {
        // Arrange
        var command = new ConfirmChangeEmailCommand("non-existent-token");

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
            It.IsAny<string>(),
            UserTokenType.EmailChange,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

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
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalidForFoundUser()
    {
        // Arrange
        var command = new ConfirmChangeEmailCommand("wrong-token");
        var user = new UserEntity("John", "john", "old@example.com", this._passwordHash);

        user.RequestEmailChange(Email.Create(PendingEmail), ConfirmToken, RevertToken, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetBySecurityTokenAsync(
                It.IsAny<string>(),
                UserTokenType.EmailChange,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await this._sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>()
            .WithMessage("Invalid or expired email change token.");

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
