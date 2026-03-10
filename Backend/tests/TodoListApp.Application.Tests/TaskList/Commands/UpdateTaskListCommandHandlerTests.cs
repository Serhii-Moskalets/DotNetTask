using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskListCommandHandler"/>.
/// Verifies validation, existence checks, and title update logic when updating a task list.
/// </summary>
public class UpdateTaskListCommandHandlerTests
{
    private static readonly TaskListTitle Title = TaskListTitle.Create("Title");

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUniqueValueService> _uniqueNameServiceMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly UpdateTaskListCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskListCommandHandlerTests"/> class.
    /// </summary>
    public UpdateTaskListCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._uniqueNameServiceMock = new Mock<IUniqueValueService>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(tl => tl.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new UpdateTaskListCommandHandler(this._uowMock.Object, this._uniqueNameServiceMock.Object);
    }

    /// <summary>
    /// Returns failure if the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        this._uowMock.Setup(
            u =>
            u.TaskLists.GetTaskListByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity)null!);

        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "NewTitle");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Task list not found.");
    }

    /// <summary>
    /// Returns success without calling uniqueness service if the new title is unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTitleIsUnchanged()
    {
        // Arrange
        var taskList = new TaskListEntity(Guid.NewGuid(), Title);

        this._uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(taskList);

        var command = new UpdateTaskListCommand(taskList.Id, Guid.NewGuid(), Title.Value);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        this._uniqueNameServiceMock.Verify(
            s => s.GetUniqueValueAsync<TaskListTitle>(
            It.IsAny<string>(),
            It.IsAny<Func<string, TaskListTitle>>(),
            It.IsAny<Func<TaskListTitle, CancellationToken, Task<bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Updates the task list title if a new title is provided and ensures uniqueness.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTitle_WhenNewTitleIsDifferent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(Guid.NewGuid(), Title);
        var newTitle = "NewTitle";
        var uniqueTitle = TaskListTitle.Create("NewTitle Unique");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(taskList.Id, userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskList);

        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var serviceMock = new Mock<IUniqueValueService>();
        serviceMock.Setup(s => s.GetUniqueValueAsync<TaskListTitle>(
                It.Is<string>(t => t == newTitle),
                It.IsAny<Func<string, TaskListTitle>>(),
                It.IsAny<Func<TaskListTitle, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(uniqueTitle);

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, serviceMock.Object);

        var command = new UpdateTaskListCommand(taskList.Id, userId, "NewTitle");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        taskList.Title.Should().Be(uniqueTitle);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
