using System.Net;
using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Validator for the <see cref="ConfirmPasswordResetCommand"/>.
/// </summary>
public class ConfirmPasswordResetCommandValidator : AbstractValidator<ConfirmPasswordResetCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPasswordResetCommandValidator"/> class.
    /// </summary>
    public ConfirmPasswordResetCommandValidator()
    {
        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage(TokenPolicy.RequiredMessage);

        this.RuleFor(x => x.NewPassword)
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
