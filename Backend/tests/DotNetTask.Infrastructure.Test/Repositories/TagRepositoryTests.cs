using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;

using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TagRepository"/>.
/// Tests cover existence checks, pagination and ownership verification.
/// </summary>
public class TagRepositoryTests
{
    private static readonly TagName TagName = TagName.Create("Tag");

    /// <summary>
    /// Tests that checking tag existence by name returns false when the tag does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByNameAsync_ReturnsFalse_WhenTagDoesNotExist()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        // Act
        bool result = await repo.ExistsByNameAsync(TagName, userId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="TagRepository.ExistsByNameAsync"/>
    /// returns true when a tag exists for a given user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrue_WhenTagExists()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid userId = Guid.NewGuid();
        TagEntity tag = new(TagName, userId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act
        bool result = await repo.ExistsByNameAsync(TagName, userId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that retrieving tags for a user returns an empty collection
    /// and zero count when no tags exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTagsAsync_ReturnsEmptyResult_WhenNoTagsExist()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        // Act
        (IReadOnlyCollection<TagEntity>? items, int totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);

        // Assert
        items.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    /// <summary>
    /// Checks that <see cref="TagRepository.GetTagsAsync"/> retrieves
    /// a paginated list of tags and the correct total count for a user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTagsAsync_ReturnsPaginatedTagsAndTotalCount()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        for (int i = 1; i <= 15; i++)
        {
            await repo.AddAsync(new TagEntity(TagName.Create($"Tag_{i:D2}"), userId));
        }

        await context.SaveChangesAsync();

        (IReadOnlyCollection<TagEntity>? items, int totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);

        // Assert
        items.Should().NotBeEmpty();
        items.Count.Should().Be(10);
        totalCount.Should().Be(15);
        items.First().Name.Value.Should().Be("Tag_01");
        items.Last().Name.Value.Should().Be("Tag_10");
    }

    /// <summary>
    /// Verifies that <see cref="TagRepository.GetTagsAsync"/> returns tags
    /// ordered by their creation date.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTagsAsync_ShouldReturnTagsOrderedByCreatedDate()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid userId = Guid.NewGuid();

        TagEntity tagOld = new(TagName.Create("Oldest"), userId) { CreatedDate = DateTime.UtcNow.AddMinutes(-10) };
        TagEntity tagMiddle = new(TagName.Create("Middle"), userId) { CreatedDate = DateTime.UtcNow.AddMinutes(-5) };
        TagEntity tagNew = new(TagName.Create("Newest"), userId) { CreatedDate = DateTime.UtcNow };

        context.Tags.AddRange(tagMiddle, tagOld, tagNew);
        await context.SaveChangesAsync();

        // Act
        (IReadOnlyCollection<TagEntity>? items, int totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);
        List<TagEntity> itemsList = items.ToList();

        // Assert
        totalCount.Should().Be(3);
        itemsList[0].Name.Value.Should().Be("Oldest");
        itemsList[1].Name.Value.Should().Be("Middle");
        itemsList[2].Name.Value.Should().Be("Newest");
    }

    /// <summary>
    /// Verifies that <see cref="TagRepository.GetTagByIdForUserAsync"/> returns the tag
    /// only if it belongs to the specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTagByIdForUserAsync_ReturnsTag_OnlyForCorrectOwner()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid ownerId = Guid.NewGuid();
        Guid strangerId = Guid.NewGuid();
        TagEntity tag = new(TagName, ownerId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act
        TagEntity? foundTag = await repo.GetTagByIdForUserAsync(tag.Id, ownerId);
        TagEntity? notFoundTag = await repo.GetTagByIdForUserAsync(tag.Id, strangerId);

        // Assert
        foundTag.Should().NotBeNull();
        foundTag.Name.Value.Should().Be(TagName.Value);
        notFoundTag.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="TagRepository.IsTagOwnerAsync"/>
    /// returns correct ownership status for a tag.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task IsTagOwnerAsync_ReturnsCorrectValue()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        Guid userId_1 = Guid.NewGuid();
        Guid userId_2 = Guid.NewGuid();
        TagEntity tag = new(TagName, userId_1);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act & Assert
        bool result1 = await repo.IsTagOwnerAsync(tag.Id, userId_1);
        bool result2 = await repo.IsTagOwnerAsync(tag.Id, userId_2);

        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }
}
