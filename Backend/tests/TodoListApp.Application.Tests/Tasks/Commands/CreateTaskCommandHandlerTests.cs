using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskCommandHandler"/>.
/// Verifies handler behavior for validation, not-found scenarios, and successful task creation.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly CreateTaskCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskCommandHandlerTests"/> class.
    /// </summary>
    public CreateTaskCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);
        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);

        this._handler = new CreateTaskCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        this._taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity?)null);

        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Title",
            }, Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    /// <summary>
    /// Ensures the handler creates a task when the command is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCreateTask_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var taskList = new TaskListEntity(userId, TaskListTitle.Create("My list"));

        this._taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(
                taskListId,
                userId,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var dto = new CreateTaskDto
        {
            DueDate = DateTime.UtcNow.AddDays(1),
            TaskListId = taskListId,
            Title = "New task",
        };

        var command = new CreateTaskCommand(dto, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        this._taskRepoMock.Verify(
            r =>
            r.AddAsync(
                It.Is<TaskEntity>(t =>
                t.OwnerId == userId &&
                t.TaskListId == taskListId &&
                t.Title.Value == "New task"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
