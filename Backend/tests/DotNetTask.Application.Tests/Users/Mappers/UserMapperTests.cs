using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Users.Mappers;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Application.Tests.Users.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="UserMapper"/> class,
/// verifying that <see cref="UserEntity"/> instances are correctly mapped to <see cref="UserBriefDto"/>.
/// </summary>
public class UserMapperTests
{
    /// <summary>
    /// Verifies that <see cref="UserMapper.Map(UserEntity)"/> correctly maps all fields
    /// when the <see cref="UserEntity.LastName"/> is not null.
    /// </summary>
    [Fact]
    public void Map_ToUserBriefDto_ShouldMapAllFields()
    {
        // Arrange
        UserEntity entity = UserEntityFactory.Create();

        // Act
        UserBriefDto userDto = UserMapper.Map(entity);

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
        UserEntity entity = new UserEntity(
            FirstName.Create("John"),
            UserName.Create("test"),
            Email.Create("john@example.com"),
            PasswordHash.Create(new string('a', 64)));

        // Act
        UserBriefDto userDto = UserMapper.Map(entity);

        // Assert
        userDto.Should().NotBeNull();
        userDto.FirstName.Should().Be(entity.FirstName.Value);
        userDto.UserName.Should().Be(entity.UserName.Value);
        userDto.Email.Should().Be(entity.Email.Value);
        userDto.LastName.Should().BeNull();
    }
}
