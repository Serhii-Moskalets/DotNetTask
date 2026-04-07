using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Test.DbContext;

/// <summary>
/// Contains unit tests for <see cref="DotNetTaskDbContext"/>.
/// Tests basic CRUD operations for all entities using an in-memory database.
/// </summary>
public class DotNetTaskDbContextTests
{
    private static readonly TaskTitle TaskTitle = TaskTitle.Create("Task");
    private static readonly TaskListTitle TaskListTitle = TaskListTitle.Create("My Task List");

    /// <summary>
    /// Tests that a <see cref="UserEntity"/> can be added and retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_UserEntity()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserEntity user = UserEntityFactory.Create();

        // Act
        context.Add(user);
        context.SaveChanges();

        // Assert
        UserEntity? savedUser = context.Users.FirstOrDefault(u => u.UserName.Value == UserEntityFactory.UserName);
        savedUser.Should().NotBeNull();
        savedUser.Email.Value.Should().Be("john@example.com");
        savedUser.FirstName.Value.Should().Be("John");
    }

    /// <summary>
    /// Tests that a <see cref="TaskListEntity"/> can be added with an associated user
    /// and correctly retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_TaskListEntity_With_User()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        UserEntity user = UserEntityFactory.Create();

        context.Add(user);
        context.SaveChanges();

        TaskListEntity taskList = new(user.Id, TaskListTitle);

        // Act
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        // Assert
        TaskListEntity? savedTaskList = context.TaskLists.FirstOrDefault(tl => tl.OwnerId == user.Id);
        savedTaskList.Should().NotBeNull();
        savedTaskList.Title.Should().Be(TaskListTitle);
    }

    /// <summary>
    /// Tests that a <see cref="TaskEntity"/> can be added with an associated task list
    /// and correctly retrieved from the database, including its relationship to the task list.
    /// </summary>
    [Fact]
    public void Can_Add_TaskEntity_With_TaskList()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        TaskListEntity taskList = new(Guid.NewGuid(), TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        TaskEntity task = new(Guid.NewGuid(), taskList.Id, TaskTitle);

        // Act
        context.Tasks.Add(task);
        context.SaveChanges();

        // Assert
        TaskEntity? savedTask = context.Tasks.Include(t => t.TaskList).FirstOrDefault();
        savedTask.Should().NotBeNull();
        savedTask.Title.Should().Be(TaskTitle);
        savedTask.TaskListId.Should().Be(taskList.Id);
    }

    /// <summary>
    /// Tests that a <see cref="TagEntity"/> can be added with an associated user
    /// and correctly retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_TagEntity_With_User()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        UserEntity user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        TagEntity tag = new(TagName.Create("Tag"), user.Id);

        // Act
        context.Tags.Add(tag);
        context.SaveChanges();

        // Assert
        TagEntity? savedTag = context.Tags.FirstOrDefault(c => c.Id == tag.Id);
        savedTag.Should().NotBeNull();
        savedTag.Name.Value.Should().Be("Tag");
    }

    /// <summary>
    /// Tests that a <see cref="CommentEntity"/> can be added with an associated task,
    /// user, and task list, and correctly retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_ComentEntity_With_TaskList_User_Task()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        UserEntity user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        TaskListEntity taskList = new(user.Id, TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        TaskEntity task = new(user.Id, taskList.Id, TaskTitle);
        context.Tasks.Add(task);
        context.SaveChanges();

        CommentEntity comment = new(task.Id, user.Id, CommentContent.Create("Comment"));

        // Act
        context.Comments.Add(comment);
        context.SaveChanges();

        // Assert
        CommentEntity? savedComment = context.Comments.FirstOrDefault(c => c.Id == comment.Id);
        savedComment.Should().NotBeNull();
        savedComment.Content.Value.Should().Be("Comment");
    }

    /// <summary>
    /// Tests that a <see cref="UserTaskAccessEntity"/> can be added
    /// and correctly retrieved from the database, validating its TaskId and UserId.
    /// </summary>
    [Fact]
    public void Can_Add_UserTaskAccess()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        Guid userId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();

        UserTaskAccessEntity access = new(taskId, userId);

        // Act
        context.UserTaskAccesses.Add(access);
        context.SaveChanges();

        // Assert
        UserTaskAccessEntity? savedAccess = context.UserTaskAccesses.FirstOrDefault();
        savedAccess.Should().NotBeNull();
        savedAccess.UserId.Should().Be(userId);
        savedAccess.TaskId.Should().Be(taskId);
    }
}
