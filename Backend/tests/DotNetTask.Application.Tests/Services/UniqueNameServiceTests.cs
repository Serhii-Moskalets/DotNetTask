using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Services;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using Moq;

namespace DotNetTask.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="UniqueValueService"/>.
/// Validates that unique task list names are generated correctly for a user.
/// </summary>
public class UniqueValueServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly UniqueValueService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueValueServiceTests"/> class.
    /// Sets up mocks and the service under test.
    /// </summary>
    public UniqueValueServiceTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._unitOfWorkMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);

        this._service = new UniqueValueService();
    }

    /// <summary>
    /// Verifies that <see cref="UniqueValueService.GetUniqueValueAsync"/>
    /// returns the original title if no existing task list with the same title exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUniqueNameAsync_ReturnsOriginalTitle_WhenTitleDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string baseTitle = "Task List";

        this._taskListRepoMock
            .Setup(r => r.ExistsByTitleAsync(It.IsAny<TaskListTitle>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        TaskListTitle result = await this._service.GetUniqueValueAsync<TaskListTitle>(
            baseTitle,
            name => TaskListTitle.Create(name),
            (vo, ct) => this._unitOfWorkMock.Object.TaskLists.ExistsByTitleAsync(vo, userId, ct),
            CancellationToken.None);

        // Assert
        result.Value.Should().Be(baseTitle);
    }

    /// <summary>
    /// Verifies that <see cref="UniqueValueService.GetUniqueValueAsync"/>
    /// appends a numeric suffix to the title if the original title already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUniqueNameAsync_AppendsSuffix_WhenTitleAlreadyExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string baseTitle = "Task List";

        this._unitOfWorkMock
            .SetupSequence(x => x.TaskLists.ExistsByTitleAsync(It.IsAny<TaskListTitle>(), userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);

        // Act
        TaskListTitle result = await this._service.GetUniqueValueAsync<TaskListTitle>(
            baseTitle,
            name => TaskListTitle.Create(name),
            (vo, ct) => this._unitOfWorkMock.Object.TaskLists.ExistsByTitleAsync(vo, userId, ct),
            CancellationToken.None);

        // Assert
        result.Value.Should().Be("Task List (2)");
    }
}
