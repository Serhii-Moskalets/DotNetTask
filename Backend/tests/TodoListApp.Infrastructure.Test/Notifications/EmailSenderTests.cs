using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TodoListApp.Infrastructure.Notifications.Services;
using TodoListApp.Infrastructure.Notifications.Settings;

namespace TodoListApp.Infrastructure.Test.Notifications;

/// <summary>
/// Contains integration tests for <see cref="EmailSender"/> to verify SMTP connectivity.
/// </summary>
public class EmailSenderTests
{
    /// <summary>
    /// Verifies that the <see cref="EmailSender.SendAsync"/> method successfully sends an email.
    /// </summary>
    /// <remarks>
    /// This test supports multiple configuration sources:
    /// <list type="bullet">
    /// <item>Local: User Secrets (run 'dotnet user-secrets set "EmailSettings:Password" "..."').</item>
    /// <item>CI/CD: Environment Variables (e.g., EmailSettings__Password in GitHub Actions).</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact(Skip = "Integration test. Run manually to verify real SMTP connectivity.")]
    public async Task SendAsync_WithValidSettings_ShouldSendEmail()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<EmailSenderTests>()
            .AddEnvironmentVariables()
            .Build();

        var settings = configuration
            .GetSection("EmailSettings")
            .Get<EmailSettings>();

        // Assert
        settings.Should().NotBeNull();
        settings!.Host.Should().NotBeNullOrEmpty("Host must be loaded from local appsettings.json");

        var sender = new EmailSender(Options.Create(settings));

        // Act & Assert
        const string recipient = "s.moskalets16@gmail.com";

        var act = async () => await sender.SendAsync(recipient, "Test Subject", "Hello from Test!", CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
