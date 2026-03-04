using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Contains unit tests for <see cref="CommentRepository"/>.
/// </summary>
public class CommentRepositoryTests
{
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that <see cref="CommentRepository.GetCommentsByTaskIdAsync"/> returns an empty collection
    /// and a zero total count when no comments exist for the specified task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetCommentsByTaskIdAsync_WhenNoComments_ReturnsEmptyResult()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);
        var taskId = Guid.NewGuid();

        // Act
        var (items, totalCount) = await repo.GetCommentsByTaskIdAsync(taskId, 1, 10);

        // Assert
        Assert.Empty(items);
        Assert.Equal(0, totalCount);
    }

    /// <summary>
    /// Verifies that <see cref="CommentRepository.GetCommentsByTaskIdAsync"/> correctly includes
    /// the associated <see cref="UserEntity"/> (author) for the retrieved comments.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetCommentsByTaskIdAsync_IncludesUserEntity()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);

        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var taskId = Guid.NewGuid();
        var comment = new CommentEntity(taskId, user.Id, CommentContent.Create("Comment"));
        await repo.AddAsync(comment);
        await context.SaveChangesAsync();

        // Act
        var (items, _) = await repo.GetCommentsByTaskIdAsync(taskId, 1, 10);

        // Assert
        var result = items.First();
        Assert.NotNull(result.User);
        Assert.Equal("john", result.User.UserName.Value);
    }

    /// <summary>
    /// Verifies that <see cref="CommentRepository.GetCommentsByTaskIdAsync"/> returns the correct
    /// subset of items and total count when requested with specific pagination parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetCommentsByTaskIdAsync_ReturnsCorrectPageItems()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", this._passwordHash);
        await context.Users.AddAsync(user);

        var taskId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            var comment = new CommentEntity(taskId, user.Id, CommentContent.Create($"Text_{i}"));
            await repo.AddAsync(comment);
        }

        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetCommentsByTaskIdAsync(taskId, page: 2, pageSize: 2);

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);
        Assert.Equal("Text_3", items.First().Content.Value);
    }
}
