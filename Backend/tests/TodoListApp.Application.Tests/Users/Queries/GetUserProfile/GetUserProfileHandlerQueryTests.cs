using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Users.Queries.GetUserProfile;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Test.Common;

namespace TodoListApp.Application.Tests.Users.Queries.GetUserProfile;

/// <summary>
/// Unit tests for <see cref="GetUserProfileQueryHandler"/>.
/// </summary>
public class GetUserProfileQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetUserProfileQueryHandler _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserProfileQueryHandlerTests"/> class.
    /// </summary>
    public GetUserProfileQueryHandlerTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._sut = new GetUserProfileQueryHandler(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that the handler returns a failure result when the user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserProfileQuery(userId);

        this._unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<Guid>(), default))
                       .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await this._sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(TinyResult.Enums.ErrorCode.NotFound);
        result.Error.Message.Should().Be(UserPolicy.AccountNotFoundMessage);
    }

    /// <summary>
    /// Verifies that the handler returns a success result with mapped DTO when the user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userEntity = UserEntityFactory.Create();

        var query = new GetUserProfileQuery(userEntity.Id);

        this._unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(userEntity);

        // Act
        var result = await this._sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be(userEntity.FirstName.Value);
        result.Value.LastName.Should().Be(userEntity.LastName?.Value);
        result.Value.UserName.Should().Be(userEntity.UserName.Value);
        result.Value.Email.Should().Be(userEntity.Email.Value);
    }
}
