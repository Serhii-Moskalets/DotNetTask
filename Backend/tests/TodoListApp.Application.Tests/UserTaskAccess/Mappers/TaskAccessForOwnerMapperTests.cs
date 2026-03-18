using FluentAssertions;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Test.Common;

namespace TodoListApp.Application.Tests.UserTaskAccess.Mappers;

/// <summary>
/// Unit tests for the <see cref="TaskAccessForOwnerMapper"/> class.
/// </summary>
public class TaskAccessForOwnerMapperTests
{
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that a single <see cref="UserTaskAccessEntity"/> is correctly mapped
    /// to a <see cref="UserBriefDto"/>, specifically checking the nested User properties.
    /// </summary>
    [Fact]
    public void Map_SingleEntity_ShouldMapAllRequiredFieldsFromUser()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var user = UserEntityFactory.Create();

        var entity = new UserTaskAccessEntity(taskId, user.Id)
        {
            User = user,
        };

        // Act
        var result = TaskAccessForOwnerMapper.Map(entity);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be(user.FirstName.Value);
        result.LastName.Should().Be(user.LastName!.Value);
        result.UserName.Should().Be(user.UserName.Value);
        result.Email.Should().Be(user.Email.Value);
    }

    /// <summary>
    /// Verifies that a collection of <see cref="UserTaskAccessEntity"/> is correctly mapped
    /// to a collection of <see cref="UserBriefDto"/>.
    /// </summary>
    [Fact]
    public void Map_Collection_ShouldReturnMappedList()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var entities = new List<UserTaskAccessEntity>
        {
            new(taskId, Guid.NewGuid())
            {
                User = UserEntityFactory.Create(),
            },
            new(taskId, Guid.NewGuid())
            {
                User = UserEntityFactory.Create("Rick", "ricky", "rick@test.com"),
            },
        };

        // Act
        var result = TaskAccessForOwnerMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First();
        var firstEntity = entities[0];

        firstResult.Id.Should().Be(firstEntity.User.Id);
        firstResult.Email.Should().Be(firstEntity.User.Email.Value);
    }

    /// <summary>
    /// Verifies that mapping an empty collection returns an empty list instead of null.
    /// </summary>
    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var entities = new List<UserTaskAccessEntity>();

        // Act
        var result = TaskAccessForOwnerMapper.Map(entities);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
