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
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        UserEntity user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

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
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        UserEntity user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        TaskListEntity taskList = new TaskListEntity(user.Id, TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

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
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        TaskListEntity taskList = new TaskListEntity(Guid.NewGuid(), TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        TaskEntity task = new TaskEntity(Guid.NewGuid(), taskList.Id, TaskTitle);
        context.Tasks.Add(task);
        context.SaveChanges();

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
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        UserEntity user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        TagEntity tag = new TagEntity(TagName.Create("Tag"), user.Id);
        context.Tags.Add(tag);
        context.SaveChanges();

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
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        UserEntity user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        TaskListEntity taskList = new TaskListEntity(user.Id, TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        TaskEntity task = new TaskEntity(user.Id, taskList.Id, TaskTitle);
        context.Tasks.Add(task);
        context.SaveChanges();

        CommentEntity comment = new CommentEntity(task.Id, user.Id, CommentContent.Create("Comment"));
        context.Comments.Add(comment);
        context.SaveChanges();

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
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();

        Guid userId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();

        UserTaskAccessEntity access = new UserTaskAccessEntity(taskId, userId);
        context.UserTaskAccesses.Add(access);
        context.SaveChanges();

        UserTaskAccessEntity? savedAccess = context.UserTaskAccesses.FirstOrDefault();
        savedAccess.Should().NotBeNull();
        savedAccess.UserId.Should().Be(userId);
        savedAccess.TaskId.Should().Be(taskId);
    }
}
