using FluentAssertions;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandHandler"/>.
/// Verifies validation, user existence, and task list creation logic.
/// </summary>
public class CreateTaskListCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUniqueNameService> _uniqueNameServiceMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly CreateTaskListCommandHandler _handler;
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskListCommandHandlerTests"/> class.
    /// </summary>
    public CreateTaskListCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._uniqueNameServiceMock = new Mock<IUniqueNameService>();
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
        var userId = Guid.NewGuid();

        this._uowMock.Setup(u => u.Users.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((UserEntity)null!);

        var command = new CreateTaskListCommand(userId, "My Task List");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Be("User not found.");
    }

    /// <summary>
    /// Creates a task list successfully when validation passes and the user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCreateTaskList_WhenNameIsUnique()
    {
        // Arrange
        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);

        this._uowMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        this._uowMock.Setup(u => u.TaskLists.AddAsync(It.IsAny<TaskListEntity>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        this._uniqueNameServiceMock.Setup(s => s.GetUniqueNameAsync(
            It.IsAny<string>(),
            It.IsAny<Func<string, CancellationToken, Task<bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("My Task List");

        var command = new CreateTaskListCommand(user.Id, "My Task List");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this._uowMock.Verify(
            u => u.TaskLists.AddAsync(
                It.Is<TaskListEntity>(t => t.OwnerId == user.Id && t.Title.Value == "My Task List"),
                It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
