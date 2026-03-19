using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Validator for <see cref="CreateUserTaskAccessCommand"/>.
/// </summary>
public class CreateUserTaskAccessCommandValidator : AbstractValidator<CreateUserTaskAccessCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserTaskAccessCommandValidator"/> class
    /// and configures validation rules for creating user-task access.
    /// </summary>
    public CreateUserTaskAccessCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage(UserPolicy.IdRequiredMessage);

        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(EmailPolicy.EmptyMessage)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(EmailPolicy.InvalidFormatMessage);
    }
}
