using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;

using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TaskListRepository"/> to verify its CRUD and query operations.
/// Includes checks for existence, retrieval, paging, sorting, and ownership verification.
/// </summary>
public class TaskListRepositoryTests
{
    private static readonly TaskListTitle TaskListTitleA = TaskListTitle.Create("Task_List_Title_A");
    private static readonly TaskListTitle TaskListTitleB = TaskListTitle.Create("Task_List_Title_B");
    private static readonly TaskListTitle TaskListTitleC = TaskListTitle.Create("Task_List_Title_C");

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.ExistsByTitleAsync"/>
    /// returns true when a task list exists for the user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByTitle_ReturnTrue_WhenTaskListExists()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);

        Guid userId = Guid.NewGuid();
        TaskListEntity taskList = new(userId, title: TaskListTitleA);

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        // Act
        bool exists = await repo.ExistsByTitleAsync(TaskListTitleA, userId);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.ExistsByTitleAsync"/>
    /// returns false when a task list does not exist for the user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByTitle_ReturnFalse_WhenTaskListDoesNotExists()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        // Act
        bool exists = await repo.ExistsByTitleAsync(TaskListTitleA, userId);

        // Assert
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListsAsync"/> returns a paged result
    /// with the correct items and total count.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTaskListsAsync_ShouldReturnPagedResult_WithCorrectCount()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        List<TaskListEntity> taskLists =
        [
            new (userId, TaskListTitleA),
            new (userId, TaskListTitleB),
            new (userId, TaskListTitleC),
        ];

        await context.AddRangeAsync(taskLists);
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskListEntity>? items, int totalCount) = await repo.GetTaskListsAsync(userId, page: 1, pageSize: 2);

        // Assert
        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
        items.ElementAt(0).Title.Should().Be(TaskListTitleA);
        items.ElementAt(1).Title.Should().Be(TaskListTitleB);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListsAsync"/> returns only
    /// lists belonging to the specified user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTaskListsAsync_ShouldReturnOnlyUserSpecificLists()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);
        Guid user1 = Guid.NewGuid();
        Guid user2 = Guid.NewGuid();

        await context.AddRangeAsync(
            new TaskListEntity(user1, TaskListTitleA),
            new TaskListEntity(user2, TaskListTitleB));
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskListEntity>? items, int totalCount) = await repo.GetTaskListsAsync(user1, 1, 10);

        // Assert
        totalCount.Should().Be(1);
        items.Should().ContainSingle()
             .Which.OwnerId.Should().Be(user1);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListsAsync"/> returns task lists
    /// ordered by their creation date.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTaskListsAsync_ShouldReturnListsOrderedByCreatedDate()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        TaskListEntity listOld = new(userId, TaskListTitleA) { CreatedDate = DateTime.UtcNow.AddMinutes(-10) };
        TaskListEntity listMiddle = new(userId, TaskListTitleB) { CreatedDate = DateTime.UtcNow.AddMinutes(-5) };
        TaskListEntity listNew = new(userId, TaskListTitleC) { CreatedDate = DateTime.UtcNow };

        await context.AddRangeAsync(listMiddle, listNew, listOld);
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TaskListEntity>? items, int totalCount) = await repo.GetTaskListsAsync(userId, page: 1, pageSize: 10);
        List<TaskListEntity> itemsList = items.ToList();

        // Assert
        items.Should().BeInAscendingOrder(x => x.CreatedDate);
        items.Select(x => x.Title).Should().Equal(TaskListTitleA, TaskListTitleB, TaskListTitleC);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListByIdForUserAsync"/>
    /// retrieves the correct task list by its ID.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUser_ReturnTaskList_WhenTaskListExists()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);

        Guid userId = Guid.NewGuid();
        TaskListEntity taskList = new(userId, title: TaskListTitleA);

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        // Act
        TaskListEntity? saved = await repo.GetTaskListByIdForUserAsync(taskList.Id, userId);

        // Assert
        saved.Should().NotBeNull();
        saved!.Title.Should().Be(taskList.Title);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListByIdForUserAsync"/>
    /// returns null when the task list does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUser_ReturnTaskListEmpty_WhenTaskListDoesNotExists()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);

        Guid userId = Guid.NewGuid();
        Guid taskListId = Guid.NewGuid();

        // Act
        TaskListEntity? saved = await repo.GetTaskListByIdForUserAsync(taskListId, userId);

        // Assert
        saved.Should().BeNull();
    }

    /// <summary>
    /// Checks that <see cref="TaskListRepository.GetTaskListByIdForUserAsync"/>
    /// returns null when the task list does not belong to the specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTaskListByIdForUserAsync_ReturnsNull_WhenUserMismatch()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TaskListRepository repo = new(context);

        Guid user1 = Guid.NewGuid();
        Guid user2 = Guid.NewGuid();
        TaskListEntity taskList = new(user1, title: TaskListTitleA);
        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        // Act
        TaskListEntity? result = await repo.GetTaskListByIdForUserAsync(taskList.Id, user2);

        // Assert
        result.Should().BeNull();
    }
}
