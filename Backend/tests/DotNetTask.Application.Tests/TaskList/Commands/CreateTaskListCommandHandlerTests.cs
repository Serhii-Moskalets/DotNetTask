using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.TaskList.Commands.CreateTaskList;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandHandler"/>.
/// Verifies validation, user existence, and task list creation logic.
/// </summary>
public class CreateTaskListCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUniqueValueService> _uniqueNameServiceMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly CreateTaskListCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskListCommandHandlerTests"/> class.
    /// </summary>
    public CreateTaskListCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._uniqueNameServiceMock = new Mock<IUniqueValueService>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(tl => tl.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new CreateTaskListCommandHandler(this._uowMock.Object, this._uniqueNameServiceMock.Object);
    }

    /// <summary>
    /// Returns NotFound if the specified user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        this._uowMock.Setup(u => u.Users.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((UserEntity)null!);

        CreateTaskListCommand command = new CreateTaskListCommand(userId, "My Task List");

        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be(UserPolicy.AccountNotFoundMessage);
    }

    /// <summary>
    /// Creates a task list successfully when validation passes and the user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCreateTaskList_WhenNameIsUnique()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        TaskListTitle uniqueTaskListTitle = TaskListTitle.Create("My Task List");

        this._uowMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        this._uowMock.Setup(u => u.TaskLists.AddAsync(It.IsAny<TaskListEntity>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        this._uniqueNameServiceMock.Setup(s => s.GetUniqueValueAsync<TaskListTitle>(
            It.IsAny<string>(),
            It.IsAny<Func<string, TaskListTitle>>(),
            It.IsAny<Func<TaskListTitle, CancellationToken, Task<bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(uniqueTaskListTitle);

        CreateTaskListCommand command = new CreateTaskListCommand(user.Id, "My Task List");

        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._uowMock.Verify(
            u => u.TaskLists.AddAsync(
                It.Is<TaskListEntity>(t => t.OwnerId == user.Id && t.Title.Value == "My Task List"),
                It.IsAny<CancellationToken>()), Times.Once);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
