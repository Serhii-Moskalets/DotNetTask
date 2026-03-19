using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Test.Common;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.DbContext;

/// <summary>
/// Contains unit tests for <see cref="TodoListAppDbContext"/>.
/// Tests basic CRUD operations for all entities using an in-memory database.
/// </summary>
public class TodoListAppDbContextTests
{
    private static readonly TaskTitle TaskTitle = TaskTitle.Create("Task");
    private static readonly TaskListTitle TaskListTitle = TaskListTitle.Create("My Task List");

    /// <summary>
    /// Tests that a <see cref="UserEntity"/> can be added and retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_UserEntity()
    {
        using var context = InMemoryDbContextFactory.Create();

        var user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        var savedUser = context.Users.FirstOrDefault(u => u.UserName.Value == UserEntityFactory.UserName);
        Assert.NotNull(savedUser);
        Assert.Equal("john@example.com", savedUser.Email.Value);
        Assert.Equal("John", savedUser.FirstName.Value);
    }

    /// <summary>
    /// Tests that a <see cref="TaskListEntity"/> can be added with an associated user
    /// and correctly retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_TaskListEntity_With_User()
    {
        using var context = InMemoryDbContextFactory.Create();

        var user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        var taskList = new TaskListEntity(user.Id, TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        var savedTaskList = context.TaskLists.FirstOrDefault(tl => tl.OwnerId == user.Id);
        Assert.NotNull(savedTaskList);
        Assert.Equal(TaskListTitle, savedTaskList.Title);
    }

    /// <summary>
    /// Tests that a <see cref="TaskEntity"/> can be added with an associated task list
    /// and correctly retrieved from the database, including its relationship to the task list.
    /// </summary>
    [Fact]
    public void Can_Add_TaskEntity_With_TaskList()
    {
        using var context = InMemoryDbContextFactory.Create();

        var taskList = new TaskListEntity(Guid.NewGuid(), TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        var task = new TaskEntity(Guid.NewGuid(), taskList.Id, TaskTitle);
        context.Tasks.Add(task);
        context.SaveChanges();

        var savedTask = context.Tasks.Include(t => t.TaskList).FirstOrDefault();
        Assert.NotNull(savedTask);
        Assert.Equal(TaskTitle, savedTask.Title);
        Assert.Equal(taskList.Id, savedTask.TaskListId);
    }

    /// <summary>
    /// Tests that a <see cref="TagEntity"/> can be added with an associated user
    /// and correctly retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_TagEntity_With_User()
    {
        using var context = InMemoryDbContextFactory.Create();

        var user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        var tag = new TagEntity(TagName.Create("Tag"), user.Id);
        context.Tags.Add(tag);
        context.SaveChanges();

        var savedTag = context.Tags.FirstOrDefault(c => c.Id == tag.Id);
        Assert.NotNull(savedTag);
        Assert.Equal("Tag", savedTag.Name.Value);
    }

    /// <summary>
    /// Tests that a <see cref="CommentEntity"/> can be added with an associated task,
    /// user, and task list, and correctly retrieved from the database.
    /// </summary>
    [Fact]
    public void Can_Add_ComentEntity_With_TaskList_User_Task()
    {
        using var context = InMemoryDbContextFactory.Create();

        var user = UserEntityFactory.Create();
        context.Add(user);
        context.SaveChanges();

        var taskList = new TaskListEntity(user.Id, TaskListTitle);
        context.TaskLists.Add(taskList);
        context.SaveChanges();

        var task = new TaskEntity(user.Id, taskList.Id, TaskTitle);
        context.Tasks.Add(task);
        context.SaveChanges();

        var comment = new CommentEntity(task.Id, user.Id, CommentContent.Create("Comment"));
        context.Comments.Add(comment);
        context.SaveChanges();

        var savedComment = context.Comments.FirstOrDefault(c => c.Id == comment.Id);
        Assert.NotNull(savedComment);
        Assert.Equal("Comment", savedComment.Content.Value);
    }

    /// <summary>
    /// Tests that a <see cref="UserTaskAccessEntity"/> can be added
    /// and correctly retrieved from the database, validating its TaskId and UserId.
    /// </summary>
    [Fact]
    public void Can_Add_UserTaskAccess()
    {
        using var context = InMemoryDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var access = new UserTaskAccessEntity(taskId, userId);
        context.UserTaskAccesses.Add(access);
        context.SaveChanges();

        var savedAccess = context.UserTaskAccesses.FirstOrDefault();
        Assert.NotNull(savedAccess);
        Assert.Equal(userId, savedAccess.UserId);
        Assert.Equal(taskId, savedAccess.TaskId);
    }
}
