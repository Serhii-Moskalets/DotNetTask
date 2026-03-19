using FluentValidation.TestHelper;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Tests for <see cref="DeleteTaskAccessesByUserCommandValidator"/>.
/// </summary>
public class DeleteTaskAccessesByUserCommandValidatorTests
{
    private readonly DeleteTaskAccessesByUserCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new DeleteTaskAccessesByUserCommand(Guid.Empty);
        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when the user identifier is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        var command = new DeleteTaskAccessesByUserCommand(Guid.NewGuid());
        var result = this._validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
