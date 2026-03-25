using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Contains unit tests for <see cref="CommentRepository"/>.
/// </summary>
public class CommentRepositoryTests
{
    /// <summary>
    /// Verifies that <see cref="CommentRepository.GetCommentsByTaskIdAsync"/> returns an empty collection
    /// and a zero total count when no comments exist for the specified task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetCommentsByTaskIdAsync_WhenNoComments_ReturnsEmptyResult()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        CommentRepository repo = new CommentRepository(context);
        Guid taskId = Guid.NewGuid();

        // Act
        (IReadOnlyCollection<CommentEntity>? items, int totalCount) = await repo.GetCommentsByTaskIdAsync(taskId, 1, 10);

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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        CommentRepository repo = new CommentRepository(context);

        UserEntity user = UserEntityFactory.Create();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        Guid taskId = Guid.NewGuid();
        CommentEntity comment = new CommentEntity(taskId, user.Id, CommentContent.Create("Comment"));
        await repo.AddAsync(comment);
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<CommentEntity>? items, int _) = await repo.GetCommentsByTaskIdAsync(taskId, 1, 10);

        // Assert
        CommentEntity result = items.First();
        Assert.NotNull(result.User);
        Assert.Equal(UserEntityFactory.UserName, result.User.UserName.Value);
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
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        CommentRepository repo = new CommentRepository(context);
        UserEntity user = UserEntityFactory.Create();
        await context.Users.AddAsync(user);

        Guid taskId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            CommentEntity comment = new CommentEntity(taskId, user.Id, CommentContent.Create($"Text_{i}"));
            await repo.AddAsync(comment);
        }

        await context.SaveChangesAsync();

        (IReadOnlyCollection<CommentEntity>? items, int totalCount) = await repo.GetCommentsByTaskIdAsync(taskId, page: 2, pageSize: 2);

        // Assert
        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);
        Assert.Equal("Text_3", items.First().Content.Value);
    }
}
