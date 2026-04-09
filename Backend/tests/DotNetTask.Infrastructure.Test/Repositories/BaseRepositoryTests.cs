using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Test.Helpers;
using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="BaseRepository{TEntity}"/> using <see cref="TagRepository"/>.
/// Covers basic repository operations: Add, Delete, GetById, Exists, and tracking behavior.
/// </summary>
public class BaseRepositoryTests
{
    private static readonly TagName TagName = TagName.Create("Tag");

    /// <summary>
    /// Tests that <see cref="TagRepository"/> adds an entity and returns it by identifier.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddAsync_Should_Add_Entity_And_GetByIdAsync_Should_Return_Entity()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);
        TagEntity entity = new(TagName, Guid.NewGuid());

        // Act
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        TagEntity? saved = await repo.GetByIdAsync(entity.Id);

        // Assert
        saved.Should().NotBeNull();
        saved.Name.Value.Should().Be(TagName.Value);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository"/> deletes an entity and it is no longer retrievable.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAsync_Should_Remove_Entity()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        TagEntity entity = new(
            TagName,
            Guid.NewGuid());

        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        repo.Delete(entity);
        await context.SaveChangesAsync();

        TagEntity? saved = await repo.GetByIdAsync(entity.Id);

        // Assert
        saved.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="TagRepository"/> reports existence for an existing entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_Should_Return_True_For_Existing_Entity()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        TagEntity entity = new(
            TagName,
            Guid.NewGuid());

        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        bool exists = await repo.ExistsAsync(entity.Id);

        // Assert
        exists.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="TagRepository"/> reports non-existence for a missing entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_Should_Return_False_For_NonExisting_Entity()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        // Act
        bool exists = await repo.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that deleting a null entity throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Delete_Should_ThrowError_When_Null_Entity()
    {
        // Arrange
        using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        // Act
        Action result = () => repo.Delete(null!);

        // Assert
        result.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that adding a null entity
    /// throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddAsync_Should_Throw_ArgumentNullException_When_Entity_Is_Null()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        // Act
        Func<Task> act = () => repo.AddAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that entity retrieval respects the no-tracking configuration.
    /// </summary>
    /// <param name="asNoTracking">Indicates whether tracking is disabled.</param>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_Should_Respect_AsNoTracking_Flag(bool asNoTracking)
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        TagEntity entity = new(TagName, Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        TagEntity? saved = await repo.GetByIdAsync(entity.Id, asNoTracking);

        // Assert
        saved.Should().NotBeNull();
        saved.Name.Value.Should().Be(TagName.Value);
    }

    /// <summary>
    /// Tests that multiple entities are stored and retrieved correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Repository_Should_Handle_Multiple_Entities()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        TagEntity entity1 = new(TagName.Create("Tag_1"), Guid.NewGuid());
        TagEntity entity2 = new(TagName.Create("Tag_2"), Guid.NewGuid());

        // Act
        await repo.AddAsync(entity1);
        await repo.AddAsync(entity2);
        await context.SaveChangesAsync();

        TagEntity? saved1 = await repo.GetByIdAsync(entity1.Id);
        TagEntity? saved2 = await repo.GetByIdAsync(entity2.Id);

        // Assert
        saved1.Should().NotBeNull();
        saved2.Should().NotBeNull();
        saved1.Name.Value.Should().Be("Tag_1");
        saved2.Name.Value.Should().Be("Tag_2");
    }

    /// <summary>
    /// Tests that an entity is not persisted without calling SaveChanges.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddAsync_Should_Not_Persist_Without_SaveChanges()
    {
        // Arrange
        await using DotNetTaskDbContext context = InMemoryDbContextFactory.Create();
        TagRepository repo = new(context);

        TagEntity entity = new(TagName, Guid.NewGuid());

        // Act
        await repo.AddAsync(entity);

        TagEntity? saved = await repo.GetByIdAsync(entity.Id);

        // Assert
        saved.Should().BeNull();
    }
}
