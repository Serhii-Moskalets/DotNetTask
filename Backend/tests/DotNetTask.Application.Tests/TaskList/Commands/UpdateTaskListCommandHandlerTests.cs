using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.TaskList.Commands.UpdateTaskList;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

using TinyResult;

namespace DotNetTask.Application.Tests.TaskList.Commands;

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

        UpdateTaskListCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "NewTitle");

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(TaskListPolicy.NotFoundMessage);
    }

    /// <summary>
    /// Returns success without calling uniqueness service if the new title is unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTitleIsUnchanged()
    {
        // Arrange
        TaskListEntity taskList = new(Guid.NewGuid(), Title);

        this._uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(taskList);

        UpdateTaskListCommand command = new(taskList.Id, Guid.NewGuid(), Title.Value);

        // Act
        Result<bool> result = await this._handler.Handle(command, CancellationToken.None);

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
        Guid userId = Guid.NewGuid();
        TaskListEntity taskList = new(Guid.NewGuid(), Title);
        string newTitle = "NewTitle";
        TaskListTitle uniqueTitle = TaskListTitle.Create("NewTitle Unique");

        Mock<IUnitOfWork> uowMock = new();
        uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(taskList.Id, userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskList);

        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Mock<IUniqueValueService> serviceMock = new();
        serviceMock.Setup(s => s.GetUniqueValueAsync<TaskListTitle>(
                It.Is<string>(t => t == newTitle),
                It.IsAny<Func<string, TaskListTitle>>(),
                It.IsAny<Func<TaskListTitle, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(uniqueTitle);

        UpdateTaskListCommandHandler handler = new(uowMock.Object, serviceMock.Object);

        UpdateTaskListCommand command = new(taskList.Id, userId, "NewTitle");

        // Act
        Result<bool> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        taskList.Title.Should().Be(uniqueTitle);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
