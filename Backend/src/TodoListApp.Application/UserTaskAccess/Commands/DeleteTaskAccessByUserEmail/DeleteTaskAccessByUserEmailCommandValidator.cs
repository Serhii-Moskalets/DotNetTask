using FluentValidation;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessByUserEmailCommand"/>.
/// </summary>
public partial class DeleteTaskAccessByUserEmailCommandValidator
    : AbstractValidator<DeleteTaskAccessByUserEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByUserEmailCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessByUserEmailCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
             .NotEmpty().WithMessage(TaskPolicy.IdRequiredMessage);

        this.RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage(UserTaskAccessPolicy.OwnerIdRequired);

        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(EmailPolicy.EmptyMessage)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(EmailPolicy.InvalidFormatMessage);
    }
}
