using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Tests for <see cref="UserTaskAccessRepository"/>.
/// Verifies adding, deleting, checking existence, and retrieving user-task access records.
/// </summary>
public class UserTaskAccessRepositoryTests
{
    private static readonly TaskTitle TaskTitle1 = TaskTitle.Create("Task title 1");
    private static readonly TaskTitle TaskTitle2 = TaskTitle.Create("Task title 2");
    private static readonly TaskListTitle TaskListTitle = TaskListTitle.Create("Task list title");

    /// <summary>
    /// Verifies that a user-task access entry can be added
    /// and then retrieved by task ID and user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Add_GetByTaskAndUserIdAsync_ShouldAddUserTaskAccessAndGetByTaskAndUserId()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user_1 = UserEntityFactory.Create();
        await context.Users.AddAsync(user_1);
        UserEntity user_2 = UserEntityFactory.Create("rick", "rickky", "rick@example.com");
        await context.Users.AddAsync(user_2);

        TaskListEntity taskList = new(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        UserTaskAccessEntity access = new(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Act
        UserTaskAccessEntity? saved = await repo.GetByTaskAndUserIdAsync(task.Id, user_2.Id);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user_1 = UserEntityFactory.Create();
        await context.Users.AddAsync(user_1);
        UserEntity user_2 = UserEntityFactory.Create("rick", "rickky", "rick@example.com");
        await context.Users.AddAsync(user_2);

        TaskListEntity taskList = new(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        UserTaskAccessEntity access = new(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        // Act
        int deleted = await repo.DeleteByIdAsync(task.Id, user_2.Id);
        bool act = await context.UserTaskAccesses.AnyAsync();

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user_1 = UserEntityFactory.Create();
        await context.Users.AddAsync(user_1);
        UserEntity user_2 = UserEntityFactory.Create("rick", "rickky", "rick@example.com");
        await context.Users.AddAsync(user_2);

        TaskListEntity taskList = new(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task_1 = new(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task_1);

        TaskEntity task_2 = new(user_2.Id, taskList.Id, TaskTitle2);
        await context.Tasks.AddAsync(task_2);

        UserTaskAccessEntity access_1 = new(task_1.Id, user_2.Id);
        await repo.AddAsync(access_1);

        UserTaskAccessEntity access_2 = new(task_2.Id, user_1.Id);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        // Act
        int deleted = await repo.DeleteAllByTaskIdAsync(task_1.Id);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user_1 = UserEntityFactory.Create();
        await context.Users.AddAsync(user_1);
        UserEntity user_2 = UserEntityFactory.Create("rick", "rickky", "rick@example.com");
        await context.Users.AddAsync(user_2);

        TaskListEntity taskList = new(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task_1 = new(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task_1);

        TaskEntity task_2 = new(user_2.Id, taskList.Id, TaskTitle2);
        await context.Tasks.AddAsync(task_2);

        UserTaskAccessEntity access_1 = new(task_1.Id, user_2.Id);
        await repo.AddAsync(access_1);

        UserTaskAccessEntity access_2 = new(task_2.Id, user_1.Id);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        // Act
        int deleted = await repo.DeleteAllByUserIdAsync(user_2.Id);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity owner = UserEntityFactory.Create();
        await context.Users.AddAsync(owner);

        TaskListEntity taskList = new(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(owner.Id, taskList.Id, TaskTitle1) { CreatedDate = DateTime.UtcNow };
        await context.Tasks.AddAsync(task);

        for (int i = 0; i < 15; i++)
        {
            UserEntity user = UserEntityFactory.Create($"User{i}", $"user{i}", $"u{i}@ex.com");
            await context.Users.AddAsync(user);
            await repo.AddAsync(new UserTaskAccessEntity(task.Id, user.Id));
        }

        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<UserTaskAccessEntity>? items, int totalCount) = await repo.GetUserTaskAccessByTaskIdAsync(task.Id, page: 1, pageSize: 10);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity owner = UserEntityFactory.Create("Owner", "owner", "owner@example.com");
        UserEntity sharedUser = UserEntityFactory.Create("Shared", "shared", "shared@example.com");
        await context.Users.AddRangeAsync(owner, sharedUser);

        TaskListEntity list = new(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(list);

        for (int i = 0; i < 5; i++)
        {
            TaskEntity t = new(owner.Id, list.Id, TaskTitle.Create($"Task{i}")) { CreatedDate = DateTime.UtcNow.AddMinutes(i) };
            await context.Tasks.AddAsync(t);
            await repo.AddAsync(new UserTaskAccessEntity(t.Id, sharedUser.Id));
        }

        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<UserTaskAccessEntity>? items, int totalCount) = await repo.GetSharedTasksByUserIdAsync(sharedUser.Id, page: 2, pageSize: 2);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user = UserEntityFactory.Create();
        await context.Users.AddAsync(user);
        TaskListEntity list = new(user.Id, TaskListTitle);
        await context.TaskLists.AddAsync(list);

        TaskEntity oldTask = new(user.Id, list.Id, TaskTitle1) { CreatedDate = DateTime.UtcNow.AddDays(-1) };
        TaskEntity newTask = new(user.Id, list.Id, TaskTitle2) { CreatedDate = DateTime.UtcNow };

        await context.Tasks.AddRangeAsync(oldTask, newTask);
        await repo.AddAsync(new UserTaskAccessEntity(oldTask.Id, user.Id));
        await repo.AddAsync(new UserTaskAccessEntity(newTask.Id, user.Id));
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<UserTaskAccessEntity>? items, int _) = await repo.GetSharedTasksByUserIdAsync(user.Id, page: 1, pageSize: 10);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity owner = UserEntityFactory.Create("Owner", "owner", "owner@example.com");
        UserEntity sharedUser = UserEntityFactory.Create("Shared", "shared", "shared@example.com");
        await context.Users.AddRangeAsync(owner, sharedUser);

        TaskListEntity taskList = new(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task1 = new(owner.Id, taskList.Id, TaskTitle1) { CreatedDate = DateTime.UtcNow };
        TaskEntity task2 = new(owner.Id, taskList.Id, TaskTitle2) { CreatedDate = DateTime.UtcNow.AddMinutes(1) };
        await context.Tasks.AddRangeAsync(task1, task2);

        UserTaskAccessEntity access1 = new(task1.Id, sharedUser.Id);
        UserTaskAccessEntity access2 = new(task2.Id, sharedUser.Id);
        await repo.AddAsync(access1);
        await repo.AddAsync(access2);
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<UserTaskAccessEntity>? items, int totalCount) = await repo.GetSharedTasksByUserIdAsync(sharedUser.Id);

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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user = UserEntityFactory.Create("SharedUser", "user", "user@e.com");
        UserEntity owner = UserEntityFactory.Create("Owner", "owner", "owner@e.com");
        await context.Users.AddRangeAsync(user, owner);

        TaskListEntity list = new(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(list);

        TaskTitle title3 = TaskTitle.Create("Task Title 3");

        TaskEntity task1 = new(owner.Id, list.Id, TaskTitle1);
        TaskEntity task2 = new(owner.Id, list.Id, TaskTitle2);
        TaskEntity task3 = new(owner.Id, list.Id, title3);
        await context.Tasks.AddRangeAsync(task1, task2, task3);

        UserTaskAccessEntity accessOld = new(task1.Id, user.Id) { CreatedDate = DateTime.UtcNow.AddHours(-2) };
        UserTaskAccessEntity accessMiddle = new(task2.Id, user.Id) { CreatedDate = DateTime.UtcNow.AddHours(-1) };
        UserTaskAccessEntity accessNew = new(task3.Id, user.Id) { CreatedDate = DateTime.UtcNow };

        await context.UserTaskAccesses.AddRangeAsync(accessOld, accessMiddle, accessNew);
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<UserTaskAccessEntity>? items, int _) = await repo.GetSharedTasksByUserIdAsync(user.Id, page: 1, pageSize: 10);

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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user_1 = UserEntityFactory.Create("John", "john", "john@example.com");
        await context.Users.AddAsync(user_1);
        UserEntity user_2 = UserEntityFactory.Create("John2", "john2", "john2@example.com");
        await context.Users.AddAsync(user_2);

        TaskListEntity taskList = new(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        UserTaskAccessEntity access = new(task.Id, user_2.Id);
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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user_1 = UserEntityFactory.Create("John", "john", "john@example.com");
        await context.Users.AddAsync(user_1);
        UserEntity user_2 = UserEntityFactory.Create("John2", "john2", "john2@example.com");
        await context.Users.AddAsync(user_2);

        TaskListEntity taskList = new(user_1.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(user_1.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        UserTaskAccessEntity access = new(task.Id, user_2.Id);
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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user = UserEntityFactory.Create();
        await context.Users.AddAsync(user);

        TaskListEntity taskList = new(user.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(user.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        UserTaskAccessEntity access = new(task.Id, user.Id);
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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity user = UserEntityFactory.Create();
        await context.Users.AddAsync(user);

        TaskListEntity taskList = new(user.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);

        TaskEntity task = new(user.Id, taskList.Id, TaskTitle1);
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
        await using DotNetTaskDbContext context = SqliteInMemoryDbContextFactory.Create();
        UserTaskAccessRepository repo = new(context);

        UserEntity owner = UserEntityFactory.Create();
        await context.Users.AddAsync(owner);
        TaskListEntity taskList = new(owner.Id, TaskListTitle);
        await context.TaskLists.AddAsync(taskList);
        TaskEntity task = new(owner.Id, taskList.Id, TaskTitle1);
        await context.Tasks.AddAsync(task);

        string[] userNames = ["Zebra", "Alice", "Charlie", "Bob"];
        foreach (string? name in userNames)
        {
            UserEntity user = UserEntityFactory.Create(name, name.ToLower(), $"{name}@ex.com");
            await context.Users.AddAsync(user);
            await repo.AddAsync(new UserTaskAccessEntity(task.Id, user.Id));
        }

        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<UserTaskAccessEntity>? items, int _) = await repo.GetUserTaskAccessByTaskIdAsync(task.Id, page: 1, pageSize: 10);

        // Assert
        items.Select(x => x.User.FirstName.Value)
             .Should().ContainInConsecutiveOrder("Alice", "Bob", "Charlie", "Zebra");
    }
}
