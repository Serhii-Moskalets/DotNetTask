using System.Net;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.RegisterUser;

/// <summary>
/// Validator for the <see cref="RegisterUserCommand"/>.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidator"/> class.
    /// </summary>
    public RegisterUserCommandValidator()
    {
        this.RuleFor(x => x.FirstName)
            .NotEmpty()
                .WithMessage(FirstNamePolicy.EmptyMessage)
            .MaximumLength(FirstName.MaxLength)
                .WithMessage(FirstNamePolicy.TooLongMessage);

        this.RuleFor(x => x.LastName)
            .MaximumLength(LastName.MaxLength)
                .WithMessage(LastNamePolicy.TooLongMessage);

        this.RuleFor(x => x.UserName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(UserNamePolicy.EmptyMessage)
            .Length(UserName.MinLength, UserName.MaxLength)
                .WithMessage(UserNamePolicy.LengthMessage)
            .Matches(UserNamePolicy.FormatRegex)
                .WithMessage(UserNamePolicy.InvalidCharactersMessage);

        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(EmailPolicy.EmptyMessage)
            .MaximumLength(Email.MaxLength)
                .WithMessage(EmailPolicy.TooLongMessage)
            .Matches(EmailPolicy.FormatRegex)
                .WithMessage(EmailPolicy.InvalidFormatMessage);

        this.RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(PasswordPolicy.EmptyMessage)
            .MinimumLength(PasswordPolicy.MinLength)
                .WithMessage(PasswordPolicy.TooShortMessage)
            .Matches(PasswordPolicy.UppercaseRegex)
                .WithMessage(PasswordPolicy.UppercaseMessage)
            .Matches(PasswordPolicy.LowercaseRegex)
                .WithMessage(PasswordPolicy.LowercaseMessage)
            .Matches(PasswordPolicy.NumberRegex)
                .WithMessage(PasswordPolicy.NumberMessage)
            .Matches(PasswordPolicy.SpecialCharRegex)
                .WithMessage(PasswordPolicy.SpecialCharMessage);

        this.RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage(CommonPolicy.InvalidIpAddressMessage)
            .Must(ip =>
            {
                bool isHexOrFourPart = ip.Contains(':') || ip.Split('.').Length == 4;
                return isHexOrFourPart && IPAddress.TryParse(ip, out _);
            })
            .WithMessage(CommonPolicy.InvalidIpAddressMessage);
    }
}
