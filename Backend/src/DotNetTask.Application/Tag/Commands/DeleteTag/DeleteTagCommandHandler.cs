using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Handles the <see cref="DeleteTagCommand"/> by deleting a tag that belongs to a specific user.
/// </summary>
public class DeleteTagCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTagCommand, Result<bool>>
{
    /// <summary>
    /// Processes the command to delete a user's tag.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the tag to delete and the user ID of the owner.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the tag was deleted,
    /// or failure if the tag was not found or the user is not the owner.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        TagEntity? tag = await this.UnitOfWork.Tags
            .GetTagByIdForUserAsync(command.TagId, command.UserId, asNoTracking: false, cancellationToken);
        if (tag is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TagPolicy.NotFoundMessage);
        }

        this.UnitOfWork.Tags.Delete(tag);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
