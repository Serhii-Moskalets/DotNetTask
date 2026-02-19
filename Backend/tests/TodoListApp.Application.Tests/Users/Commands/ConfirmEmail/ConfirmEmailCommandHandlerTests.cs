using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Commands.ConfirmEmail;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmEmailCommandHandler"/> class.
/// </summary>
public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfirmEmailCommandHandler _sut;
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmEmailCommandHandlerTests"/> class.
    /// </summary>
    public ConfirmEmailCommandHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new ConfirmEmailCommandHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that a valid token successfully confirms the email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string token = "valid-token";
        var command = new ConfirmEmailCommand(userId, token);
        var user = new UserEntity("John", "john", "test@example.com", this._passwordHash);

        user.RequestEmailVerification(token, TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await this._sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
        var command = new ConfirmEmailCommand(Guid.NewGuid(), "any-token");

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
        var command = new ConfirmEmailCommand(userId, "wrong-token");
        var user = new UserEntity("John", "john", "test@example.com", this._passwordHash);

        user.RequestEmailVerification("valid-token", TimeSpan.FromHours(1));

        this._unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => this._sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid or expired email verification token.");

        this._unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
