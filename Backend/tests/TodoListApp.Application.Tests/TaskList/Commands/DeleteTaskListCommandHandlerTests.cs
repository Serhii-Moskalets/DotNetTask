using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskListCommandHandler"/>.
/// Verifies validation, existence, and deletion behavior for task lists.
/// </summary>
public class DeleteTaskListCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly DeleteTaskListCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskListCommandHandlerTests"/> class.
    /// </summary>
    public DeleteTaskListCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(tl => tl.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new DeleteTaskListCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns failure if the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        this._taskListRepoMock.Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((TaskListEntity?)null);

        var command = new DeleteTaskListCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Task list not found.");
    }

    /// <summary>
    /// Deletes the task list when validation passes and it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTaskList_WhenValidationPasses()
    {
        // Arrange
        var taskList = new TaskListEntity(Guid.NewGuid(), TaskListTitle.Create("Test TaskList"));

        this._taskListRepoMock.Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(taskList);
        this._taskListRepoMock.Setup(r => r.DeleteAsync(taskList, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteTaskListCommand(taskList.Id, Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._taskListRepoMock.Verify(r => r.DeleteAsync(taskList, It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
