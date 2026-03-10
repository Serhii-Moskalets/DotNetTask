using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Tests for <see cref="UserTaskAccessRepository"/>.
/// Verifies adding, deleting, checking existence, and retrieving user-task access records.
/// </summary>
public class UserTaskAccessRepositoryTests
{
    private static readonly TaskTitle TaskTitle1 = TaskTitle.Create("Task title 1");
    private static readonly TaskTitle TaskTitle2 = TaskTitle.Create("Task title 2");
    private static readonly TaskListTitle TaskListTitle = TaskListTitle.Create("Task list title");
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that a user-task access entry can be added
    /// and then retrieved by task ID and user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Add_GetByTaskAndUserIdAsync_ShouldAddUserTaskAccessAndGetByTaskAndUserId()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash);
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Act
        var saved = await repo.GetByTaskAndUserIdAsync(task.Id, user_2.Id);

        // Assert
        saved.Should().NotBeNull();
        task.Id.Should().Be(saved.TaskId);
        user_2.Id.Should().Be(saved.UserId);
    }

    /// <summary>
    /// Verifies that a specific user-task access entry
    /// can be removed by task ID and user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteByTaskAndUserIdAsync_ShouldRemoveAccess()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash);
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Act
        var deleted = await repo.DeleteByIdAsync(task.Id, user_2.Id);
        var act = await context.UserTaskAccesses.AnyAsync();

        // Assert
        deleted.Should().Be(1);
        act.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that all access entries related to a specific task
    /// are removed when deleting by task ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAllByTaskIdAsync_ShouldRemoveAccess()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash);
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task_1 = new TaskEntity(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task_1);

        var task_2 = new TaskEntity(user_2.Id, taskList.Id, TaskTitle2);
        await context.Tasks.AddAsync(task_2);

        var access_1 = new UserTaskAccessEntity(task_1.Id, user_2.Id);
        await repo.AddAsync(access_1);

        var access_2 = new UserTaskAccessEntity(task_2.Id, user_1.Id);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        // Act
        var deleted = await repo.DeleteAllByTaskIdAsync(task_1.Id);

        deleted.Should().Be(1);
        (await repo.ExistsAsync(task_1.Id, user_2.Id)).Should().BeFalse();
        (await repo.ExistsAsync(task_2.Id, user_1.Id)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that all access entries related to a specific user
    /// are removed when deleting by user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAllByUserIdAsync_ShouldRemoveAccess()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash);
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task_1 = new TaskEntity(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task_1);

        var task_2 = new TaskEntity(user_2.Id, taskList.Id, TaskTitle2);
        await context.Tasks.AddAsync(task_2);

        var access_1 = new UserTaskAccessEntity(task_1.Id, user_2.Id);
        await repo.AddAsync(access_1);

        var access_2 = new UserTaskAccessEntity(task_2.Id, user_1.Id);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        // Act
        var deleted = await repo.DeleteAllByUserIdAsync(user_2.Id);

        deleted.Should().Be(1);
        (await repo.ExistsAsync(task_1.Id, user_2.Id)).Should().BeFalse();
        (await repo.ExistsAsync(task_2.Id, user_1.Id)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the repository correctly implements pagination and
    /// includes task navigation properties when fetching users for a specific task.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUserTaskAccessByTaskIdAsync_ShouldReturnPaginatedAccessList()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var owner = new UserEntity("Owner", "owner", "owner@example.com", this._passwordHash);
        await context.Users.AddAsync(owner);

        var taskList = new TaskListEntity(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(owner.Id, taskList.Id, TaskTitle1) { CreatedDate = DateTime.UtcNow };
        await context.Tasks.AddAsync(task);

        for (int i = 0; i < 15; i++)
        {
            var user = new UserEntity($"User{i}", $"user{i}", $"u{i}@ex.com", this._passwordHash);
            await context.Users.AddAsync(user);
            await repo.AddAsync(new UserTaskAccessEntity(task.Id, user.Id));
        }

        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetUserTaskAccessByTaskIdAsync(task.Id, page: 1, pageSize: 10);

        // Assert
        totalCount.Should().Be(15);
        items.Should().HaveCount(10);
        items.Should().AllSatisfy(x =>
        {
            x.User.Should().NotBeNull();
            x.TaskId.Should().Be(task.Id);
        });
    }

    /// <summary>
    /// Verifies that pagination offsets (skipping pages) work correctly when
    /// retrieving tasks shared with a specific user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetSharedTasksByUserIdAsync_ShouldReturnCorrectPagination()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var owner = new UserEntity("Owner", "owner", "owner@example.com", this._passwordHash);
        var sharedUser = new UserEntity("Shared", "shared", "shared@example.com", this._passwordHash);
        await context.Users.AddRangeAsync(owner, sharedUser);

        var list = new TaskListEntity(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(list);

        for (int i = 0; i < 5; i++)
        {
            var t = new TaskEntity(owner.Id, list.Id, TaskTitle.Create($"Task{i}")) { CreatedDate = DateTime.UtcNow.AddMinutes(i) };
            await context.Tasks.AddAsync(t);
            await repo.AddAsync(new UserTaskAccessEntity(t.Id, sharedUser.Id));
        }

        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetSharedTasksByUserIdAsync(sharedUser.Id, page: 2, pageSize: 2);

        // Assert
        totalCount.Should().Be(5);
        items.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that shared tasks are returned in descending order based
    /// on their creation date (newest first).
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetSharedTasksByUserIdAsync_ShouldReturnCorrectOrder()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user = new UserEntity("User", "user", "u@e.com", this._passwordHash);
        await context.Users.AddAsync(user);
        var list = new TaskListEntity(user.Id, TaskListTitle);
        await context.TaskLists.AddAsync(list);

        var oldTask = new TaskEntity(user.Id, list.Id, TaskTitle1) { CreatedDate = DateTime.UtcNow.AddDays(-1) };
        var newTask = new TaskEntity(user.Id, list.Id, TaskTitle2) { CreatedDate = DateTime.UtcNow };

        await context.Tasks.AddRangeAsync(oldTask, newTask);
        await repo.AddAsync(new UserTaskAccessEntity(oldTask.Id, user.Id));
        await repo.AddAsync(new UserTaskAccessEntity(newTask.Id, user.Id));
        await context.SaveChangesAsync();

        // Act
        var (items, _) = await repo.GetSharedTasksByUserIdAsync(user.Id, page: 1, pageSize: 10);

        // Assert
        items.First().Task.Title.Should().Be(TaskTitle2);
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.GetSharedTasksByUserIdAsync"/>
    /// returns all tasks shared with a specific user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetSharedTasksByUserIdAsync_ShouldReturnCorrectTasks()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var owner = new UserEntity("Owner", "owner", "owner@example.com", this._passwordHash);
        var sharedUser = new UserEntity("Shared", "shared", "shared@example.com", this._passwordHash);
        await context.Users.AddRangeAsync(owner, sharedUser);

        var taskList = new TaskListEntity(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task1 = new TaskEntity(owner.Id, taskList.Id, TaskTitle1) { CreatedDate = DateTime.UtcNow };
        var task2 = new TaskEntity(owner.Id, taskList.Id, TaskTitle2) { CreatedDate = DateTime.UtcNow.AddMinutes(1) };
        await context.Tasks.AddRangeAsync(task1, task2);

        var access1 = new UserTaskAccessEntity(task1.Id, sharedUser.Id);
        var access2 = new UserTaskAccessEntity(task2.Id, sharedUser.Id);
        await repo.AddAsync(access1);
        await repo.AddAsync(access2);
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetSharedTasksByUserIdAsync(sharedUser.Id);

        // Assert
        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
        items.Should().AllSatisfy(t => t.UserId.Should().Be(sharedUser.Id));
    }

    /// <summary>
    /// Verifies that shared tasks are returned in descending order (newest first)
    /// based on the CreatedDate of the access record.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetSharedTasksByUserIdAsync_ShouldReturnNewestFirst()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user = new UserEntity("SharedUser", "user", "user@e.com", this._passwordHash);
        var owner = new UserEntity("Owner", "owner", "owner@e.com", this._passwordHash);
        await context.Users.AddRangeAsync(user, owner);

        var list = new TaskListEntity(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(list);

        var title3 = TaskTitle.Create("Task Title 3");

        var task1 = new TaskEntity(owner.Id, list.Id, TaskTitle1);
        var task2 = new TaskEntity(owner.Id, list.Id, TaskTitle2);
        var task3 = new TaskEntity(owner.Id, list.Id, title3);
        await context.Tasks.AddRangeAsync(task1, task2, task3);

        var accessOld = new UserTaskAccessEntity(task1.Id, user.Id) { CreatedDate = DateTime.UtcNow.AddHours(-2) };
        var accessMiddle = new UserTaskAccessEntity(task2.Id, user.Id) { CreatedDate = DateTime.UtcNow.AddHours(-1) };
        var accessNew = new UserTaskAccessEntity(task3.Id, user.Id) { CreatedDate = DateTime.UtcNow };

        await context.UserTaskAccesses.AddRangeAsync(accessOld, accessMiddle, accessNew);
        await context.SaveChangesAsync();

        // Act
        var (items, _) = await repo.GetSharedTasksByUserIdAsync(user.Id, page: 1, pageSize: 10);

        // Assert
        items.Select(x => x.Task.Title).Should().ContainInConsecutiveOrder(title3, TaskTitle2, TaskTitle1);
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsAsync"/>
    /// returns true when an access entry exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenAccessExists()
    {
        // Arrenge
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash);
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Assert
        (await repo.ExistsAsync(task.Id, user_2.Id)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsAsync"/>
    /// returns <c>false</c> when no matching user-task access exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenAccessDoesNotExist()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", this._passwordHash);
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Assert
        (await repo.ExistsAsync(Guid.NewGuid(), user_2.Id)).Should().BeFalse();
        (await repo.ExistsAsync(task.Id, Guid.NewGuid())).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsByUserIdAsync"/>
    /// returns <c>true</c> when the specified user has access to at least one task.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByUserIdAsync_ShouldReturnTrue_WhenUserHasAccess()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user);

        var taskList = new TaskListEntity(user.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Assert
        (await repo.ExistsByUserIdAsync(user.Id)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsByUserIdAsync"/>
    /// returns <c>false</c> when the specified user has no task access entries.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByUserIdAsync_ShouldReturnFalse_WhenUserHasNoAccess()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        // Assert
        (await repo.ExistsByUserIdAsync(Guid.NewGuid())).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsByTaskIdAsync"/>
    /// correctly identifies whether any access entries exist for a given task.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByTaskIdAsync_ShouldReturnCorrectValues()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user);

        var taskList = new TaskListEntity(user.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        await repo.AddAsync(new UserTaskAccessEntity(task.Id, user.Id));
        await context.SaveChangesAsync();

        // Assert
        (await repo.ExistsByTaskIdAsync(task.Id)).Should().BeTrue();
        (await repo.ExistsByTaskIdAsync(Guid.NewGuid())).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the access entries are returned in alphabetical order by user's first name.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUserTaskAccessByTaskIdAsync_ShouldReturnInAlphabeticalOrder()
    {
        // Arrange
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var owner = new UserEntity("Owner", "owner", "owner@example.com", this._passwordHash);
        await context.Users.AddAsync(owner);
        var taskList = new TaskListEntity(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);
        var task = new TaskEntity(owner.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        var userNames = new[] { "Zebra", "Alice", "Charlie", "Bob" };
        foreach (var name in userNames)
        {
            var user = new UserEntity(name, name.ToLower(), $"{name}@ex.com", this._passwordHash);
            await context.Users.AddAsync(user);
            await repo.AddAsync(new UserTaskAccessEntity(task.Id, user.Id));
        }

        await context.SaveChangesAsync();

        // Act
        var (items, _) = await repo.GetUserTaskAccessByTaskIdAsync(task.Id, page: 1, pageSize: 10);

        // Assert
        items.Select(x => x.User.FirstName.Value)
             .Should().ContainInConsecutiveOrder("Alice", "Bob", "Charlie", "Zebra");
    }
}
