using FluentAssertions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(userId, title: TaskListTitleA);

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        // Act
        var exists = await repo.ExistsByTitleAsync(TaskListTitleA.Value, userId);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        // Act
        var exists = await repo.ExistsByTitleAsync(TaskListTitleA.Value, userId);

        // Assert
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Ensures <see cref="TaskListRepository.ExistsByTitleAsync"/> throws <see cref="ArgumentException"/>
    /// when title is null, empty, or whitespace.
    /// </summary>
    /// <param name="title">The title to test for null, empty, or whitespace.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ExistsByTitle_MustThrow_WhenTitleIsNullOrWhiteSpace(string? title)
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        // Act
        var act = async () => await repo.ExistsByTitleAsync(title!, userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        var taskLists = new List<TaskListEntity>
        {
            new (userId, TaskListTitleA),
            new (userId, TaskListTitleB),
            new (userId, TaskListTitleC),
        };

        await context.AddRangeAsync(taskLists);
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetTaskListsAsync(userId, page: 1, pageSize: 2);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        await context.AddRangeAsync(
            new TaskListEntity(user1, TaskListTitleA),
            new TaskListEntity(user2, TaskListTitleB));
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetTaskListsAsync(user1, 1, 10);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        var listOld = new TaskListEntity(userId, TaskListTitleA) { CreatedDate = DateTime.UtcNow.AddMinutes(-10) };
        var listMiddle = new TaskListEntity(userId, TaskListTitleB) { CreatedDate = DateTime.UtcNow.AddMinutes(-5) };
        var listNew = new TaskListEntity(userId, TaskListTitleC) { CreatedDate = DateTime.UtcNow };

        await context.AddRangeAsync(listMiddle, listNew, listOld);
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetTaskListsAsync(userId, page: 1, pageSize: 10);
        var itemsList = items.ToList();

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(userId, title: TaskListTitleA);

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        // Act
        var saved = await repo.GetTaskListByIdForUserAsync(taskList.Id, userId);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        // Act
        var saved = await repo.GetTaskListByIdForUserAsync(taskListId, userId);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var taskList = new TaskListEntity(user1, title: TaskListTitleA);
        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetTaskListByIdForUserAsync(taskList.Id, user2);

        // Assert
        result.Should().BeNull();
    }
}
