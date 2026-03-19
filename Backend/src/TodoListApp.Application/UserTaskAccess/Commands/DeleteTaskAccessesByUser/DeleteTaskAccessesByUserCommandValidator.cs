using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessesByUserCommand"/>.
/// </summary>
public class DeleteTaskAccessesByUserCommandValidator : AbstractValidator<DeleteTaskAccessesByUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByUserCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessesByUserCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);
    }
}
