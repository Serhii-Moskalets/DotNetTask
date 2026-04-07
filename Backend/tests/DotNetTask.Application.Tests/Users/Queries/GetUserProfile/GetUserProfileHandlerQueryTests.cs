using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Users.Queries.GetUserProfile;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.Users.Queries.GetUserProfile;

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
        Guid userId = Guid.NewGuid();
        GetUserProfileQuery query = new(userId);

        this._unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<Guid>(), default))
                       .ReturnsAsync((UserEntity?)null);

        // Act
        Result<UserBriefDto> result = await this._sut.Handle(query, CancellationToken.None);

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
        UserEntity userEntity = UserEntityFactory.Create();

        GetUserProfileQuery query = new(userEntity.Id);

        this._unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(userEntity);

        // Act
        Result<UserBriefDto> result = await this._sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be(userEntity.FirstName.Value);
        result.Value.LastName.Should().Be(userEntity.LastName?.Value);
        result.Value.UserName.Should().Be(userEntity.UserName.Value);
        result.Value.Email.Should().Be(userEntity.Email.Value);
    }
}
