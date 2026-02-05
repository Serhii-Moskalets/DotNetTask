using FluentAssertions;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="UserName"/> value object.
/// </summary>
public class UserNameTests
{
    /// <summary>
    /// Tests that a valid username is correctly created and trimmed.
    /// </summary>
    /// <param name="input">The raw username string input.</param>
    /// <param name="expected">The expected trimmed username string.</param>
    [Theory]
    [InlineData("john_doe", "john_doe")]
    [InlineData("Admin123", "Admin123")]
    [InlineData("  user_99  ", "user_99")]
    public void Create_Should_ReturnUserName_When_ValueIsValid(string input, string expected)
    {
        // Act
        var result = UserName.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="UserName.Create"/> throws <see cref="DomainException"/>
    /// when the input is null, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="invalidInput">The invalid string to be tested.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ValueIsEmpty(string? invalidInput)
    {
        // Act
        Action act = () => UserName.Create(invalidInput!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("User name cannot be empty.");
    }

    /// <summary>
    /// Tests that <see cref="UserName.Create"/> throws <see cref="DomainException"/>
    /// when the username length is out of range (3-20 characters).
    /// </summary>
    /// <param name="invalidLengthInput">The string with invalid length.</param>
    [Theory]
    [InlineData("ab")]
    [InlineData("this_name_is_way_too_long_for_system")]
    public void Create_Should_ThrowDomainException_When_LengthIsInvalid(string invalidLengthInput)
    {
        // Act
        Action act = () => UserName.Create(invalidLengthInput);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("User name must be between 3 and 20 characters.");
    }

    /// <summary>
    /// Tests that <see cref="UserName.Create"/> throws <see cref="DomainException"/>
    /// when the username contains forbidden characters.
    /// </summary>
    /// <param name="invalidFormatInput">The username with invalid characters.</param>
    [Theory]
    [InlineData("user!")]
    [InlineData("name with space")]
    [InlineData("email@test.com")]
    [InlineData("user-name")]
    public void Create_Should_ThrowDomainException_When_FormatIsInvalid(string invalidFormatInput)
    {
        // Act
        Action act = () => UserName.Create(invalidFormatInput);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("User name can only contain letters, numbers, and underscores.");
    }

    /// <summary>
    /// Tests that <see cref="UserName.ToString"/> returns the underlying username value.
    /// </summary>
    [Fact]
    public void ToString_Should_ReturnRawValue()
    {
        // Arrange
        const string rawName = "valid_user";
        var userName = UserName.Create(rawName);

        // Act
        var result = userName.ToString();

        // Assert
        result.Should().Be(rawName);
    }

    /// <summary>
    /// Tests that two <see cref="UserName"/> instances with the same value are considered equal.
    /// </summary>
    [Fact]
    public void UserNames_WithSameValue_Should_BeEqual()
    {
        // Arrange
        var name1 = UserName.Create("alex_smith");
        var name2 = UserName.Create("alex_smith");

        // Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }
}
