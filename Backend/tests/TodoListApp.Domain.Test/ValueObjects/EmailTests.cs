using FluentAssertions;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="Email"/> value object.
/// </summary>
public class EmailTests
{
    /// <summary>
    /// Tests that a valid email address is correctly created and converted to lower case.
    /// </summary>/// <param name="input">The raw email string input.</param>
    /// <param name="expected">The expected normalized email string.</param>
    [Theory]
    [InlineData("test@example.com", "test@example.com")]
    [InlineData("USER@Domain.com", "user@domain.com")]
    [InlineData("  space@test.com  ", "space@test.com")]
    public void Create_Should_ReturnEmail_When_ValueIsValid(string input, string expected)
    {
        // Act
        var result = Email.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that the Email class can successfully create an instance using a minimal valid email address.
    /// </summary>
    /// <remarks>This test ensures that the Email class accepts and processes minimal valid email formats,
    /// confirming correct behavior for basic valid inputs.</remarks>
    /// <param name="email">The email address to be validated and used for creating the Email instance. Must be a well-formed email address.</param>
    [Theory]
    [InlineData("a@b.c")]
    [InlineData("x@y.z")]
    public void Create_Should_Work_With_Minimal_Valid_Emails(string email)
    {
        // Act
        var restult = Email.Create(email);

        // Assert
        restult.Value.Should().Be(email);
    }

    /// <summary>
    /// Tests that <see cref="Email.Create"/> throws <see cref="DomainException"/>
    /// when the input is null, empty or whitespace.
    /// </summary>
    /// <param name="invalidInput">The invalid string to be tested.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ValueIsEmpty(string? invalidInput)
    {
        // Act
        Action act = () => Email.Create(invalidInput!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(EmailPolicy.EmptyMessage);
    }

    /// <summary>
    /// Tests that <see cref="Email.Create"/> throws <see cref="DomainException"/>
    /// when the email exceeds the character limit.
    /// </summary>
    [Fact]
    public void Create_Should_ThrowDomainException_When_EmailTooLong()
    {
        // Arrange
        var longEmail = new string('a', Email.MaxLength + 1) + "@test.com";

        // Act
        Action act = () => Email.Create(longEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(EmailPolicy.TooLongMessage);
    }

    /// <summary>
    /// Tests that <see cref="Email.Create"/> throws <see cref="DomainException"/>
    /// when the email format does not match the regex.
    /// </summary>
    /// <param name="invalidFormat">The malformed email string to test.</param>
    [Theory]
    [InlineData("plainaddress")]
    [InlineData("#@%^%#$@#$@#.com")]
    [InlineData("@example.com")]
    [InlineData("Joe Smith <email@example.com>")]
    [InlineData("email.example.com")]
    [InlineData("email@example@example.com")]
    [InlineData("email@example..com")]
    public void Create_Should_ThrowDomainException_When_FormatIsInvalid(string invalidFormat)
    {
        // Act
        Action act = () => Email.Create(invalidFormat);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(EmailPolicy.InvalidFormatMessage);
    }

    /// <summary>
    /// Verifies that a email instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        var localPartLength = Email.MaxLength - "@a.com".Length;
        var value = new string('a', localPartLength) + "@a.com";

        var result = Email.Create(value);

        result.Value.Length.Should().Be(Email.MaxLength);
    }

    /// <summary>
    /// Tests that <see cref="Email.ToString"/> returns the underlying string value.
    /// </summary>
    [Fact]
    public void ToString_Should_ReturnRawValue()
    {
        // Arrange
        const string rawEmail = "test@example.com";
        var email = Email.Create(rawEmail);

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be(rawEmail);
    }

    /// <summary>
    /// Tests that two <see cref="Email"/> instances with the same value are considered equal.
    /// </summary>
    [Fact]
    public void Emails_WithSameValue_Should_BeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@example.com");

        // Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }
}
