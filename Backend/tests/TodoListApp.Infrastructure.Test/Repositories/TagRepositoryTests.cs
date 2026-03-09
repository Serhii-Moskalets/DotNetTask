using FluentAssertions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TagRepository"/>.
/// Tests cover existence checks, pagination and ownership verification.
/// </summary>
public class TagRepositoryTests
{
    private readonly TagName _tagName = TagName.Create("Tag");

    /// <summary>
    /// Tests that checking tag existence by name returns false when the tag does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByNameAsync_ReturnsFalse_WhenTagDoesNotExist()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        // Act
        var result = await repo.ExistsByNameAsync("NonExistingTag", userId);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();
        var tag = new TagEntity(this._tagName, userId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.ExistsByNameAsync("Tag", userId);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        // Act
        var (items, totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        for (int i = 1; i <= 15; i++)
        {
            await repo.AddAsync(new TagEntity(TagName.Create($"Tag_{i:D2}"), userId));
        }

        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        var tagOld = new TagEntity(TagName.Create("Oldest"), userId) { CreatedDate = DateTime.UtcNow.AddMinutes(-10) };
        var tagMiddle = new TagEntity(TagName.Create("Middle"), userId) { CreatedDate = DateTime.UtcNow.AddMinutes(-5) };
        var tagNew = new TagEntity(TagName.Create("Newest"), userId) { CreatedDate = DateTime.UtcNow };

        context.Tags.AddRange(tagMiddle, tagOld, tagNew);
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);
        var itemsList = items.ToList();

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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var tag = new TagEntity(this._tagName, ownerId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act
        var foundTag = await repo.GetTagByIdForUserAsync(tag.Id, ownerId);
        var notFoundTag = await repo.GetTagByIdForUserAsync(tag.Id, strangerId);

        // Assert
        foundTag.Should().NotBeNull();
        foundTag.Name.Value.Should().Be(this._tagName.Value);
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
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var tag = new TagEntity(this._tagName, userId_1);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act & Assert
        var result1 = await repo.IsTagOwnerAsync(tag.Id, userId_1);
        var result2 = await repo.IsTagOwnerAsync(tag.Id, userId_2);

        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }
}
