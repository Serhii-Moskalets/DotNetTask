using System.Net;
using DotNetTask.Domain.Constants;

using FluentValidation;

namespace DotNetTask.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Validator for the <see cref="RevertEmailChangeCommand"/>.
/// </summary>
public class RevertEmailChangeCommandValidator : AbstractValidator<RevertEmailChangeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevertEmailChangeCommandValidator"/> class.
    /// </summary>
    public RevertEmailChangeCommandValidator()
    {
        this.RuleFor(x => x.Token)
            .NotEmpty().WithMessage(TokenPolicy.RequiredMessage);

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
