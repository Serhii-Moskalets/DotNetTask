using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.UpdateUserProfile;

/// <summary>
/// Command to update first and last name for current user.
/// </summary>
/// <param name="FirstName">The new user's first name.</param>
/// <param name="LastName">The new user's last name.</param>
/// <param name="UserId">The unique identifier of the user to be updated.</param>
public record UpdateUserProfileCommand(string? FirstName, string? LastName, Guid UserId)
    : ICommand<bool>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "UpdateUserProfile".</value>
    public string ActionName => "UpdateUserProfile";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
