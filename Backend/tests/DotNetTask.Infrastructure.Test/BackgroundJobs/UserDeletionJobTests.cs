using DotNetTask.Application.Users.Commands.PurgeExpiredAccounts;
using DotNetTask.Infrastructure.BackgroundJobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TinyResult;

namespace DotNetTask.Infrastructure.Test.BackgroundJobs;

/// <summary>
/// Provides unit tests for the <see cref="UserDeletionJob"/> background service.
/// Focuses on verifying the orchestration between the background worker, service scopes, and the command mediator.
/// </summary>
public class UserDeletionJobTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<ILogger<UserDeletionJob>> _loggerMock = new();
    private readonly UserDeletionJob _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDeletionJobTests"/> class.
    /// </summary>
    public UserDeletionJobTests()
    {
        Mock<IServiceScope> scope = new();
        Mock<IServiceProvider> serviceProvider = new();

        serviceProvider
            .Setup(x => x.GetService(typeof(ISender)))
            .Returns(this._senderMock.Object);

        scope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
        this._scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scope.Object);

        this._sut = new UserDeletionJob(this._scopeFactoryMock.Object, this._loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the background job successfully executes the purge command,
    /// correctly processes the result, and logs the number of deleted accounts.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ProccessDeletions_LogsCount_When_SenderSucceeds()
    {
        this._senderMock
            .Setup(x => x.Send(It.IsAny<PurgeExpiredAccountsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(3));

        using CancellationTokenSource cts = new();

        Task task = this._sut.StartAsync(cts.Token);

        await Task.Delay(50);
        cts.Cancel();
        await task;

        this._senderMock.Verify(
            x => x.Send(
                It.IsAny<PurgeExpiredAccountsCommand>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);

        this._loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Deleted 3 expired accounts")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

}
