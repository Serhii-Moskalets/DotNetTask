using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmChangeEmail;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmChangeEmailCommandHandler"/> class.
/// </summary>
public class ConfirmChangeEmailCommandHandlerTests
{
    private const string PendingEmail = "new@example.com";
    private const string Token = "valid-token";
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
    /// Verifies that a valid token successfully confirms the email change.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ConfirmChangeEmailCommand(userId, Token);
        var user = new UserEntity("John", "john", "old@example.com", this._passwordHash);

        user.RequestEmailChange(Email.Create(PendingEmail), Token, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(PendingEmail);
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that a NotFound failure is returned when the user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = new ConfirmChangeEmailCommand(Guid.NewGuid(), Token);

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that a DomainException is thrown when the token is invalid or expired.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ThrowDomainException_When_TokenIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ConfirmChangeEmailCommand(userId, "wrong-token");
        var user = new UserEntity("John", "john", "old@example.com", this._passwordHash);

        user.RequestEmailChange(Email.Create(PendingEmail), Token, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => this._sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid or expired email change token.");

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
