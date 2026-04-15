using DotNetTask.Domain.Common;
using DotNetTask.Domain.Entities;
using TinyResult;

namespace DotNetTask.Domain.Test.Common;

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
    {
        return new(
                Domain.ValueObjects.FirstName.Create(firstName ?? FirstName),
                Domain.ValueObjects.UserName.Create(userName ?? UserName),
                Domain.ValueObjects.Email.Create(email ?? Email),
                Domain.ValueObjects.PasswordHash.Create(passwordHash ?? PasswordHash),
                Domain.ValueObjects.LastName.Create(lastName ?? LastName));
    }

    /// <summary>
    /// Creates a <see cref="UserEntity"/> and moves it to the Active state by simulating the email confirmation process
    /// using the provided <see cref="FakeClock"/>.
    /// </summary>
    /// <param name="firstName">The first name of the user. Defaults to <see cref="FirstName"/>.</param>
    /// <param name="userName">The username of the user. Defaults to <see cref="UserName"/>.</param>
    /// <param name="email">The email address of the user. Defaults to <see cref="Email"/>.</param>
    /// <param name="passwordHash">The password hash string. Defaults to <see cref="PasswordHash"/>.</param>
    /// <param name="lastName">The optional last name of the user. Defaults to <see cref="LastName"/>.</param>
    /// <param name="verificationDelay">Optional delay between request and confirmation. Defaults to 10 minutes.</param>
    /// <returns>An active <see cref="UserEntity"/> with cleared domain events.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the confirmation process fails during test setup.</exception>
    public static UserEntity CreateActive(
        string? firstName = null,
        string? userName = null,
        string? email = null,
        string? passwordHash = null,
        string? lastName = null,
        TimeSpan? verificationDelay = null)
    {
        UserEntity user = Create(
            firstName,
            userName,
            email,
            passwordHash,
            lastName);

        FakeClock clock = new(TestTime.Now);
        TimeSpan delay = verificationDelay ?? TimeSpan.FromMinutes(10);
        user.RequestEmailVerification("init_token", TimeSpan.FromHours(1), clock.UtcNow);

        clock.Advance(delay);

        Result<Unit> result = user.ConfirmEmailVerification("init_token", clock.UtcNow);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Test setup failed: {result.Error!.Message}");
        }

        user.ClearDomainEvents();

        return user;
    }
}
