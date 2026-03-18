using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Test.Common;

/// <summary>
/// A factory for creating <see cref="UserEntity"/> instances with default or overridden values for testing.
/// </summary>
public static class UserEntityFactory
{
    /// <summary>Default first name for test users.</summary>
    public const string FirstName = "John";

    /// <summary>Default last name for test users.</summary>
    public const string LastName = "Doe";

    /// <summary>Default username for test users.</summary>
    public const string UserName = "jdoe";

    /// <summary>Default email for test users.</summary>
    public const string Email = "john@example.com";

    /// <summary>Default password hash for test users.</summary>
    public static readonly string PasswordHash = new('a', 64);

    /// <summary>
    /// Creates a new <see cref="UserEntity"/> with provided or default values.
    /// </summary>
    /// <param name="firstName">The first name of the user. Defaults to <see cref="FirstName"/>.</param>
    /// <param name="userName">The username of the user. Defaults to <see cref="UserName"/>.</param>
    /// <param name="email">The email address of the user. Defaults to <see cref="Email"/>.</param>
    /// <param name="passwordHash">The password hash string. Defaults to <see cref="PasswordHash"/>.</param>
    /// <param name="lastName">The optional last name of the user. Defaults to <see cref="LastName"/>.</param>
    /// <returns>A fully initialized <see cref="UserEntity"/>.</returns>
    public static UserEntity Create(
        string? firstName = null,
        string? userName = null,
        string? email = null,
        string? passwordHash = null,
        string? lastName = null)
        => new(
            Domain.ValueObjects.FirstName.Create(firstName ?? FirstName),
            Domain.ValueObjects.UserName.Create(userName ?? UserName),
            Domain.ValueObjects.Email.Create(email ?? Email),
            Domain.ValueObjects.PasswordHash.Create(passwordHash ?? PasswordHash),
            Domain.ValueObjects.LastName.Create(lastName ?? LastName));
}
