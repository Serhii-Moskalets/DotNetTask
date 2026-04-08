using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Test.Common;
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

    private UserEntity _user1 = null!;
    private UserEntity _user2 = null!;
    private TaskListEntity _taskList = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskRepositoryTests"/> class.
    /// </summary>
    public TaskRepositoryTests()
    {
        this._context = SqliteInMemoryDbContextFactory.Create();
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
        await this.InitializeAsync();

        DateTime pastTime = DateTime.UtcNow.AddMinutes(5);
        DateTime now = DateTime.UtcNow.AddMinutes(20);

        TaskEntity[] tasks =
        [
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task_1"), pastTime),
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task_2"), pastTime),
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task_3"), now.AddMinutes(10)),
        ];

        foreach (TaskEntity? task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        // Act
        int count = await this._repo.CountOverdueTasksAsync(this._user1.Id, this._taskList.Id, now);

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
        await this.InitializeAsync();

        TaskEntity task = new(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task"));

        await this._repo.AddAsync(task);
        await this._context.SaveChangesAsync();

        // Act & Assert
        (await this._repo.IsTaskOwnerAsync(task.Id, this._user1.Id)).Should().BeTrue();
        (await this._repo.IsTaskOwnerAsync(task.Id, this._user2.Id)).Should().BeFalse();
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
        await this.InitializeAsync();

        for (int i = 1; i <= 5; i++)
        {
            TaskEntity task = new(this._user1.Id, this._taskList.Id, TaskTitle.Create($"Task_{i}"), DateTime.UtcNow.AddDays(i));
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity>? items, int total) = await this._repo.GetTasksAsync(
            this._user1.Id,
            this._taskList.Id,
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
        await this.InitializeAsync();

        TaskEntity[] tasks =
        [
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Apple")),
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Application")),
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Banana")),
        ];

        foreach (TaskEntity? task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity>? items, int total) = await this._repo.SearchByTitleAsync(this._user1.Id, "App", page: 1, pageSize: 10);

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
        await this.InitializeAsync();

        await this._repo.AddAsync(new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task_1")));
        await this._repo.AddAsync(new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task_2")));
        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity> _, int total1) = await this._repo.GetTasksAsync(this._user1.Id, this._taskList.Id, statuses: null);
        (IReadOnlyCollection<TaskEntity>? items2, int _) = await this._repo.GetTasksAsync(this._user1.Id, this._taskList.Id, statuses: []);

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
        await this.InitializeAsync();

        await this._repo.AddAsync(new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("B-Task")));
        await this._repo.AddAsync(new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("A-Task")));
        await this._context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskEntity>? items, int _) = await this._repo.GetTasksAsync(this._user1.Id, this._taskList.Id, sortBy: TaskSortBy.Title, ascending: true);

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
        await this.InitializeAsync();

        TaskEntity task = new(this._user1.Id, this._taskList.Id, TaskTitle.Create("Task"));

        await this._repo.AddAsync(task);
        await this._context.SaveChangesAsync();

        // Act
        TaskEntity? result = await this._repo.GetTaskByIdForUserAsync(task.Id, this._user2.Id);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that deleting a range of tasks returns the correct count and removes the specified tasks from the data
    /// store.
    /// </summary>
    /// <remarks>This test ensures that the repository's DeleteRangeAsync method deletes only the specified
    /// tasks and that the number of deleted tasks matches the expected count.</remarks>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteRangeTask_ShouldReturnCorrect_When_DeletesTasksSuccessfully()
    {
        // Arrange
        await this.InitializeAsync();

        TaskEntity[] tasks =
        [
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Apple")),
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Application")),
            new TaskEntity(this._user1.Id, this._taskList.Id, TaskTitle.Create("Banana")),
        ];

        foreach (TaskEntity? task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        Guid[] taskIdsToDelete = tasks.Take(2).Select(t => t.Id).ToArray();
        int expectedCount = taskIdsToDelete.Length;

        // Act
        int deletedTask = await this._repo.DeleteRangeAsync(taskIdsToDelete);

        // Assert
        deletedTask.Should().Be(expectedCount);

        List<TaskEntity> remainingTasks = this._context.Tasks.Where(t => taskIdsToDelete.Contains(t.Id)).ToList();
        remainingTasks.Should().BeEmpty();
    }

    /// <summary>
    /// Asynchronously initializes the test data by creating and saving users and a task list to the database context.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    private async Task InitializeAsync()
    {
        this._user1 = UserEntityFactory.Create();
        this._user2 = UserEntityFactory.Create(userName: "john", email: "johnson@example.com");

        this._taskList = new TaskListEntity(this._user1.Id, TaskListTitle.Create("Test List"));

        await this._context.Users.AddRangeAsync(this._user1, this._user2);
        await this._context.TaskLists.AddAsync(this._taskList);
        await this._context.SaveChangesAsync();
    }
}
