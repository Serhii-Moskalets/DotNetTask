using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.Users.Queries.GetUserProfile;

/// <summary>
/// Represents a query to retrieve a user's brief profile information by their unique identifier.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose profile is being requested.</param>
public record GetUserProfileQuery(Guid UserId) : IQuery<UserBriefDto>;
