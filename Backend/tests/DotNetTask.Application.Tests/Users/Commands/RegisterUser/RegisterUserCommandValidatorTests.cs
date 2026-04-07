using DotNetTask.Application.Users.Commands.RegisterUser;
using DotNetTask.Domain.Constants;
using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.RegisterUser;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandValidator"/> class.
/// </summary>
public class RegisterUserCommandValidatorTests
{
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Email = "john@test.com";
    private const string UserName = "johndoe";
    private const string Password = "Password123!";
    private const string IpAddress = "192.168.0.1";
    private readonly RegisterUserCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidatorTests"/> class.
    /// </summary>
    public RegisterUserCommandValidatorTests() => this._validator = new RegisterUserCommandValidator();

    /// <summary>
    /// Provides invalid password samples and their corresponding expected error messages from <see cref="PasswordPolicy"/>.
    /// </summary>
    /// <returns>An enumeration of test data arrays.</returns>
    public static TheoryData<string, string> GetInvalidPasswords()
    {
        return new TheoryData<string, string>
        {
            { "short", PasswordPolicy.TooShortMessage },
            { "nocapital1!", PasswordPolicy.UppercaseMessage },
            { "NOLOWERCASE1!", PasswordPolicy.LowercaseMessage },
            { "NoDigit!!", PasswordPolicy.NumberMessage },
            { "NoSpecialChar1", PasswordPolicy.SpecialCharMessage },
        };
    }

    /// <summary>
    /// Verifies that valid registration data passes all validation rules.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        RegisterUserCommand command = CreateCommend();

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that empty mandatory fields trigger validation errors.
    /// </summary>
    /// <param name="firstName">The first name to validate.</param>
    /// <param name="userName">The username to validate.</param>
    /// <param name="email">The email to validate.</param>
    /// <param name="password">The password to validate.</param>
    [Theory]
    [InlineData("", "", "", "")]
    public void Should_Have_Errors_When_Fields_Are_Empty(string firstName, string userName, string email, string password)
    {
        // Arrange
        RegisterUserCommand command = new(firstName, LastName, userName, email, password, IpAddress);

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(FirstNamePolicy.EmptyMessage);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(UserNamePolicy.EmptyMessage);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(EmailPolicy.EmptyMessage);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(PasswordPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that validation errors are triggered when mandatory fields exceed their maximum allowed length.
    /// </summary>
    [Fact]
    public void Should_Have_Errors_When_Names_Exceed_Maximum_Length()
    {
        // Arrange
        string longFirstName = new('a', Domain.ValueObjects.FirstName.MaxLength + 1);
        string longLastName = new('a', Domain.ValueObjects.LastName.MaxLength + 1);
        RegisterUserCommand command = CreateCommend(firstName: longFirstName, lastName: longLastName);

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(FirstNamePolicy.TooLongMessage);
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithErrorMessage(LastNamePolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that UserName length restrictions are enforced.
    /// </summary>
    /// <param name="userName">The username with invalid length.</param>
    [Theory]
    [InlineData("jo")]
    [InlineData("thisusernameiswaytoolongforoursystem")]
    public void Should_Have_Error_When_UserName_Length_Is_Invalid(string userName)
    {
        // Arrange
        RegisterUserCommand command = CreateCommend(userName: userName);

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(UserNamePolicy.LengthMessage);
    }

    /// <summary>
    /// Verifies that <see cref="UserName"/> triggers a format error message when it contains invalid characters.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserName_Format_Is_Invalid()
    {
        // Arrange
        RegisterUserCommand command = CreateCommend(userName: "user@name!");

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(UserNamePolicy.InvalidCharactersMessage);
    }

    /// <summary>
    /// Verifies that each password complexity rule (length, casing, digits, special characters) is correctly enforced.
    /// </summary>
    /// <param name="password">The password that fails complexity rules.</param>
    /// <param name="expectedErrorMessage">The policy-defined error message expected for the failure.</param>
    [Theory]
    [MemberData(nameof(GetInvalidPasswords))]
    public void Should_Have_Error_When_Password_Complexity_Is_Not_Met(string password, string expectedErrorMessage)
    {
        // Arrange
        RegisterUserCommand command = CreateCommend(password: password);

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// Verifies that an invalid IP address format triggers a validation error.
    /// </summary>
    /// <param name="invalidIp">The malformed IP address string.</param>
    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("256.256.256.256")]
    [InlineData("192.168.1")]
    [InlineData("...")]
    public void Should_Have_Error_When_IpAddress_Is_Invalid(string invalidIp)
    {
        // Arrange
        RegisterUserCommand command = CreateCommend(ipAddress: invalidIp);

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }

    /// <summary>
    /// Verifies that an empty IP address triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_IpAddress_Is_Empty()
    {
        // Arrange
        RegisterUserCommand command = CreateCommend(ipAddress: string.Empty);

        // Act
        TestValidationResult<RegisterUserCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }

    private static RegisterUserCommand CreateCommend(
        string? firstName = null,
        string? lastName = null,
        string? userName = null,
        string? email = null,
        string? password = null,
        string? ipAddress = null)
    {
        return new RegisterUserCommand(
            firstName ?? FirstName,
            lastName ?? LastName,
            userName ?? UserName,
            email ?? Email,
            password ?? Password,
            ipAddress ?? IpAddress);
    }
}
