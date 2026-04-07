using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TagEntity"/> domain entity.
/// </summary>
public class TagEntityTests
{
    private readonly TagName _name = TagName.Create("tag");

    /// <summary>
    /// Verifies that the constructor creates a tag
    /// when valid name and user ID are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTag_WhenValidData()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        TagEntity tag = new(this._name, userId);

        tag.Should().NotBeNull();
        tag.UserId.Should().Be(userId);
        tag.Name.Should().Be(this._name);
    }

    /// <summary>
    /// Verifies that the tasks collection is initialized
    /// when the tag is created.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeTasksCollection()
    {
        // Arrange & Act
        TagEntity tag = new(this._name, Guid.NewGuid());

        // Assert
        tag.Should().NotBeNull();
        tag.Tasks.Should().NotBeNull();
        tag.Tasks.Should().BeEmpty();
    }
}
