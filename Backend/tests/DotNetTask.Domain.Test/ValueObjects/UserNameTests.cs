using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

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
        UserName result = UserName.Create(input);

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
        // Arrange
        Action act = () => UserName.Create(invalidInput!);

        // Act & Assert
        act.Should().Throw<DomainException>()
            .WithMessage(UserNamePolicy.EmptyMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserName.Create"/> throws <see cref="DomainException"/>
    /// when the username length is out of range (<see cref="UserName.MinLength"/> - <see cref="UserName.MaxLength"/> characters).
    /// </summary>
    [Fact]
    public void Create_Should_ThrowDomainException_When_LengthIsLessThanMinLength()
    {
        // Arrange
        Action act = () => UserName.Create(new string('A', UserName.MinLength - 1));

        // Act & Assert
        act.Should().Throw<DomainException>()
            .WithMessage(UserNamePolicy.LengthMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserName.Create"/> throws <see cref="DomainException"/>
    /// when the username length is out of range (<see cref="UserName.MinLength"/> - <see cref="UserName.MaxLength"/>  characters).
    /// </summary>
    [Fact]
    public void Create_Should_ThrowDomainException_When_LengthIsLongerThanMaxLength()
    {
        // Arrange & Act
        Action act = () => UserName.Create(new string('A', UserName.MaxLength + 1));

        // Act & Assert
        act.Should().Throw<DomainException>()
            .WithMessage(UserNamePolicy.LengthMessage);
    }

    /// <summary>
    /// Verifies that a UserName instance can be created when the input string meets the minimum length requirement.
    /// </summary>
    /// <remarks>This test ensures that the UserName.Create method accepts a string of exactly
    /// UserName.MinLength characters and returns a value equal to the input. It validates the boundary condition for
    /// minimum length enforcement.</remarks>
    [Fact]
    public void Create_Should_Work_When_LengthIsMin()
    {
        // Arrange
        string value = new('A', UserName.MinLength);

        // Act
        UserName result = UserName.Create(value);

        result.Value.Should().Be(value);
    }

    /// <summary>
    /// Verifies that a UserName instance can be created successfully when the input string is at the maximum allowed
    /// length.
    /// </summary>
    /// <remarks>This test ensures that the UserName.Create method accepts a string whose length equals
    /// UserName.MaxLength without throwing exceptions, and that the resulting value matches the input. Use this test to
    /// confirm boundary handling for maximum length constraints.</remarks>
    [Fact]
    public void Create_Should_Work_When_LengthIsMax()
    {
        // Arrange
        string value = new('A', UserName.MaxLength);

        // Act
        UserName result = UserName.Create(value);

        result.Value.Should().Be(value);
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
        // Arrange & Act
        Action act = () => UserName.Create(invalidFormatInput);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(UserNamePolicy.InvalidCharactersMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserName.ToString"/> returns the underlying username value.
    /// </summary>
    [Fact]
    public void ToString_Should_ReturnRawValue()
    {
        // Arrange
        const string rawName = "valid_user";
        UserName userName = UserName.Create(rawName);

        // Act
        string result = userName.ToString();

        // Assert
        result.Should().Be(rawName);
    }

    /// <summary>
    /// Tests that two <see cref="UserName"/> instances with the same value are considered equal.
    /// </summary>
    [Fact]
    public void UserNames_WithSameValue_Should_BeEqual()
    {
        // Arrange & Act
        UserName name1 = UserName.Create("alex_smith");
        UserName name2 = UserName.Create("alex_smith");

        // Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }
}
