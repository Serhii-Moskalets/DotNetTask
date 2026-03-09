using FluentAssertions;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Services;

namespace TodoListApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="UniqueNameService"/>.
/// Validates that unique task list names are generated correctly for a user.
/// </summary>
public class UniqueNameServiceTests
{
    private const string Title = "Task List";
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly UniqueNameService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueNameServiceTests"/> class.
    /// Sets up mocks and the service under test.
    /// </summary>
    public UniqueNameServiceTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._unitOfWorkMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);

        this._service = new UniqueNameService();
    }

    /// <summary>
    /// Verifies that <see cref="UniqueNameService.GetUniqueNameAsync"/>
    /// returns the original title if no existing task list with the same title exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUniqueNameAsync_ReturnsOriginalTitle_WhenTitleDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this._taskListRepoMock.Setup(r => r.ExistsByTitleAsync(Title, userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

        // Act
        var result = await this._service.GetUniqueNameAsync(
            Title,
            (name, ct) => this._unitOfWorkMock.Object.TaskLists.ExistsByTitleAsync(name, userId, ct),
            CancellationToken.None);

        // Assert
        result.Should().Be(Title);
    }

    /// <summary>
    /// Verifies that <see cref="UniqueNameService.GetUniqueNameAsync"/>
    /// appends a numeric suffix to the title if the original title already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUniqueNameAsync_AppendsSuffix_WhenTitleAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this._unitOfWorkMock.SetupSequence(x => x.TaskLists.ExistsByTitleAsync(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true) // "Task List" exists
                .ReturnsAsync(true) // "Task List (1)" exists
                .ReturnsAsync(false); // "Task List (2)" does not exists

        // Act
        var result = await this._service.GetUniqueNameAsync(
            Title,
            (name, ct) => this._unitOfWorkMock.Object.TaskLists.ExistsByTitleAsync(name, userId, ct),
            CancellationToken.None);

        // Assert
        result.Should().Be("Task List (2)");
    }
}
