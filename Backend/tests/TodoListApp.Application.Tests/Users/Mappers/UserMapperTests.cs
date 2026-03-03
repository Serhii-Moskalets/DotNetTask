using FluentAssertions;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Users.Mappers;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Users.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="UserMapper"/> class,
/// verifying that <see cref="UserEntity"/> instances are correctly mapped to <see cref="UserBriefDto"/>.
/// </summary>
public class UserMapperTests
{
    private readonly string _passwordHash = new('a', 64);

    /// <summary>
    /// Verifies that <see cref="UserMapper.Map(UserEntity)"/> correctly maps all fields
    /// when the <see cref="UserEntity.LastName"/> is not null.
    /// </summary>
    [Fact]
    public void Map_ToUserBriefDto_ShouldMapAllFields()
    {
        // Arrange
        var entity = new UserEntity("John", "username", "test@email.com", this._passwordHash, "Johnson");

        // Act
        var userDto = UserMapper.Map(entity);

        // Assert
        userDto.Should().NotBeNull();
        userDto.FirstName.Should().Be(entity.FirstName.Value);
        userDto.LastName.Should().Be(entity.LastName!.Value);
        userDto.UserName.Should().Be(entity.UserName.Value);
        userDto.Email.Should().Be(entity.Email.Value);
    }

    /// <summary>
    /// Verifies that <see cref="UserMapper.Map(UserEntity)"/> correctly maps all fields
    /// when the <see cref="UserEntity.LastName"/> is null.
    /// </summary>
    [Fact]
    public void Map_ShouldMap_ToUserBriefDto_WhenLastNameIsNull()
    {
        // Arrange
        var entity = new UserEntity("John", "username", "test@email.com", this._passwordHash);

        // Act
        var userDto = UserMapper.Map(entity);

        // Assert
        userDto.Should().NotBeNull();
        userDto.FirstName.Should().Be(entity.FirstName.Value);
        userDto.UserName.Should().Be(entity.UserName.Value);
        userDto.Email.Should().Be(entity.Email.Value);
        userDto.LastName.Should().BeNull();
    }
}
