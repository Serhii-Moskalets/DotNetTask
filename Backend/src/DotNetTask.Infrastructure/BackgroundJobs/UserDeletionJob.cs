using DotNetTask.Application.Users.Commands.PurgeExpiredAccounts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyResult;

namespace DotNetTask.Infrastructure.BackgroundJobs;

/// <summary>
/// A background worker that periodically triggers the permanent deletion
/// of accounts that have exceeded their 30-day grace period.
/// </summary>
public class UserDeletionJob(IServiceScopeFactory scopeFactory, ILogger<UserDeletionJob> logger)
    : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.ProcessDeletionsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while purging expired accounts.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task ProcessDeletionsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Purging expired accounts at {Time}", DateTimeOffset.UtcNow);

        using IServiceScope scope = scopeFactory.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Result<int> result = await sender.Send(
            new PurgeExpiredAccountsCommand(),
            cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Deleted {Count} expired accounts.", result.Value);
        }
        else
        {
            logger.LogError("Purge failed: {Message}", result.Error?.Message);
        }

    }
}
