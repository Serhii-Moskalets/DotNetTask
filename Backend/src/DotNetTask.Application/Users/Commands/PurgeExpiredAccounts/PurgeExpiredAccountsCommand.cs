using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;

namespace DotNetTask.Application.Users.Commands.PurgeExpiredAccounts;

/// <summary>
/// Command to initiate the permanent removal of user accounts that have surpassed their <see cref="UserPolicy.DeletionDelayInDays"/> deletion grace period.
/// </summary>
public record PurgeExpiredAccountsCommand : ICommand<int>;
