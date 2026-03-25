using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Users.Mappers;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.Users.Queries.GetUserProfile;

/// <summary>
/// Handles requests to retrieve a user's profile information based on the specified query.
/// </summary>
/// <remarks>This handler processes a query to obtain a user's profile by their unique identifier. If the user
/// does not exist, the handler returns a failure result with a NotFound error code. The retrieved user entity is mapped
/// to a UserBriefDto before being returned.</remarks>
/// <param name="unitOfWork">The unit of work instance used to access user data repositories.</param>
public class GetUserProfileQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetUserProfileQuery, Result<UserBriefDto>>
{
    /// <summary>
    /// Handles the <see cref="GetUserProfileQuery"/> request to retrieve user profile information.
    /// </summary>
    /// <param name="request">The query containing the user ID of the profile to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{UserBriefDto}"/> representing the outcome of the operation.
    /// Returns a success result containing the mapped <see cref="UserBriefDto"/> if the user exists,
    /// or a failure result with <see cref="TinyResult.Enums.ErrorCode.NotFound"/> if the user does not exist.
    /// </returns>>
    public async Task<Result<UserBriefDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken: cancellationToken);
        if (user is null)
        {
            return await Result<UserBriefDto>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        UserBriefDto userDto = UserMapper.Map(user);
        return await Result<UserBriefDto>.SuccessAsync(userDto);
    }
}
