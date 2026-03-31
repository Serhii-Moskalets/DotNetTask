using System.Net;
using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.ResendEmailVerification;

/// <summary>
/// Validator for <see cref="ResendEmailVerificationCommand"/>.
/// Ensures that the command contains valid data before it is processed.
/// </summary>
public class ResendEmailVerificationCommandValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResendEmailVerificationCommandValidator"/> class.
    /// </summary>
    public ResendEmailVerificationCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage(UserPolicy.IdRequiredMessage);

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
