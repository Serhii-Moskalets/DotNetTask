using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;

using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TaskRepository"/>.
/// Tests cover existence checks, overdue task handling, pagination, filtering, and ownership verification.
/// </summary>
public class TaskRepositoryTests
{
    private readonly DotNetTaskDbContext _context;
    private readonly TaskRepository _repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskRepositoryTests"/> class.
    /// </summary>
    public TaskRepositoryTests()
    {
        this._context = InMemoryDbContextFactory.Create();
        this._repo = new TaskRepository(this._context);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.CountOverdueTasksAsync"/>
    /// returns the correct number of overdue tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CountOverdueTask_ReturnsCorrectCount()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid taskListId = Guid.NewGuid();

        DateTime pastTime = DateTime.UtcNow.AddMinutes(5);
        DateTime now = DateTime.UtcNow.AddMinutes(20);

        TaskEntity[] tasks =
        [
            new TaskEntity(userId, taskListId, TaskTitle.Create("Task_1"), pastTime),
            new TaskEntity(userId, taskListId, TaskTitle.Create("Task_2"), pastTime),
            new TaskEntity(userId, taskListId, TaskTitle.Create("Task_3"), now.AddMinutes(10)),
        ];

        foreach (TaskEntity? task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        // Act
        int count = await this._repo.CountOverdueTasksAsync(userId, taskListId, now);

        // Assert
        count.Should().Be(2);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.IsTaskOwnerAsync"/>
    /// returns correct ownership status for a task.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task IsOwnerTask_ReturnsCorrectValue()
    {
        // Arrange
        Guid userId_1 = Guid.NewGuid();
        Guid userId_2 = Guid.NewGuid();
        TaskEntity task = new(userId_1, Guid.NewGuid(), TaskTitle.Create("Task"));
        await this._repo.AddAsync(task);
        await this._context.SaveChangesAsync();

        // Act & Assert
        (await this._repo.IsTaskOwnerAsync(task.Id, userId_1)).Should().BeTrue();
        (await this._repo.IsTaskOwnerAsync(task.Id, userId_2)).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.GetTasksAsync"/> correctly applies pagination parameters
    /// and returns the expected subset of tasks along with the total count.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTasksAsync_ReturnsCorrectPageAndTotalCount()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid taskListId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            TaskEntity task = new(userId, taskListId, TaskTitle.Create($"Task_{i}"), DateTime.UtcNow.AddDays(i));
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity>? items, int total) = await this._repo.GetTasksAsync(
            userId,
            taskListId,
            page: 2,
            pageSize: 2);

        // Assert
        total.Should().Be(5);
        items.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.SearchByTitleAsync"/> filters tasks by a title substring
    /// and respects pagination limits.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task SearchByTitleAsync_ReturnsPaginatedResults()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid listId = Guid.NewGuid();

        TaskEntity[] tasks =
        [
            new TaskEntity(userId, listId, TaskTitle.Create("Apple")),
            new TaskEntity(userId, listId, TaskTitle.Create("Application")),
            new TaskEntity(userId, listId, TaskTitle.Create("Banana")),
        ];

        foreach (TaskEntity? task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity>? items, int total) = await this._repo.SearchByTitleAsync(userId, "App", page: 1, pageSize: 10);

        // Assert
        total.Should().Be(2);
        items.Should().AllSatisfy(task => task.Title.Value.Should().Contain("App"));
    }

    /// <summary>
    /// Verifies that <see cref="TaskRepository.GetTasksAsync"/> ignores status filtering
    /// when the status collection is null or empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTasks_ReturnsAll_WhenStatusesNullOrEmpty()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid taskListId = Guid.NewGuid();

        await this._repo.AddAsync(new TaskEntity(userId, taskListId, TaskTitle.Create("Task_1")));
        await this._repo.AddAsync(new TaskEntity(userId, taskListId, TaskTitle.Create("Task_2")));
        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity> _, int total1) = await this._repo.GetTasksAsync(userId, taskListId, statuses: null);
        (IReadOnlyCollection<TaskEntity>? items2, int _) = await this._repo.GetTasksAsync(userId, taskListId, statuses: []);

        // Assert
        total1.Should().Be(2);
        items2.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that <see cref="TaskRepository.GetTasksAsync"/> correctly sorts tasks
    /// by their title in ascending order.
    /// </summary>
    /// <remarks>
    /// This test ensures that when <see cref="TaskSortBy.Title"/> is provided with the ascending flag,
    /// "A-Task" appears before "B-Task" in the resulting collection.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTasksAsync_ShouldSortByTitle_Ascending()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid listId = Guid.NewGuid();
        await this._repo.AddAsync(new TaskEntity(userId, listId, TaskTitle.Create("B-Task")));
        await this._repo.AddAsync(new TaskEntity(userId, listId, TaskTitle.Create("A-Task")));
        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity>? items, int _) = await this._repo.GetTasksAsync(userId, listId, sortBy: TaskSortBy.Title, ascending: true);

        // Assert
        items.First().Title.Value.Should().Be("A-Task");
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.GetTaskByIdForUserAsync"/> returns null
    /// when a user attempts to retrieve a task that belongs to a different user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTaskByIdForUser_ReturnsNull_WhenUserMismatch()
    {
        // Arrange
        Guid user1 = Guid.NewGuid();
        Guid user2 = Guid.NewGuid();
        TaskEntity task = new(user1, Guid.NewGuid(), TaskTitle.Create("Task"));
        await this._repo.AddAsync(task);
        await this._context.SaveChangesAsync();

        // Act
        TaskEntity? result = await this._repo.GetTaskByIdForUserAsync(task.Id, user2);

        // Assert
        result.Should().BeNull();
    }
}
