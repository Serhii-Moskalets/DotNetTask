using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Infrastructure.Notifications.Options;
using DotNetTask.Infrastructure.Notifications.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetTask.Infrastructure.Test.Notifications;

/// <summary>
/// Provides unit tests for <see cref="EmailService"/> to verify its coordination logic.
/// </summary>
public class EmailServiceTests
{
    private const string Email = "john@test.com";
    private const string UserName = "john";
    private const string Link = "https://link.com";
    private const string ExpectedBody = "<html>Generated Body</html>";

    private readonly Mock<IEmailSender> _senderMock;
    private readonly Mock<IEmailTemplateProvider> _templateProviderMock;

    private readonly EmailTemplatesOptions _templates;
    private readonly EmailSubjectsOptions _subjects;

    private readonly EmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailServiceTests"/> class and sets up mocks.
    /// </summary>
    public EmailServiceTests()
    {
        this._senderMock = new Mock<IEmailSender>();
        this._templateProviderMock = new Mock<IEmailTemplateProvider>();

        this._templates = new EmailTemplatesOptions
        {
            EmailVerification = "verification_tpl",
            EmailChange = "change_tpl",
            PasswordReset = "reset_tpl",
            EmailChangeSecurityAlert = "alert_tpl",
            AccountDeleted = "deleted_tpl",
        };

        this._subjects = new EmailSubjectsOptions
        {
            RegistrationConfirmation = "Confirm Registration",
            EmailChangeConfirmation = "Confirm Email Change",
            PasswordReset = "Reset Password",
            EmailChangeSecurityAlert = "Security Alert",
            AccountDeleted = "Account Deleted",
        };

        IOptions<EmailTemplatesOptions> templatesOptions = Options.Create(this._templates);
        IOptions<EmailSubjectsOptions> subjectsOptions = Options.Create(this._subjects);

        this._emailService = new EmailService(
            this._senderMock.Object,
            this._templateProviderMock.Object,
            subjectsOptions,
            templatesOptions);

        this._templateProviderMock.Setup(
            x => x.GetEmailTemplateAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(ExpectedBody);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendConfirmationEmailAsync"/> correctly orchestrates
    /// the template generation and email delivery processes.
    /// </summary>
    /// <remarks>
    /// The test ensures that:
    /// <list type="number">
    /// <item>The <see cref="IEmailTemplateProvider"/> is called with the expected template name.</item>
    /// <item>The <see cref="IEmailSender"/> receives the specific HTML body returned by the provider.</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendConfirmationEmailAsync_Should_CallProviderAndSender()
    {
        // Act
        await this._emailService.SendConfirmationEmailAsync(Email, UserName, Link, CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(
            x => x.GetEmailTemplateAsync(
                this._templates.EmailVerification,
                It.Is<Dictionary<string, string>>(
                    dict =>
                    dict["USER_NAME"] == UserName &&
                    dict["VERIFY_LINK"] == Link)),
            Times.Once);

        this._senderMock.Verify(
            x => x.SendAsync(
                Email,
                this._subjects.RegistrationConfirmation,
                ExpectedBody,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendEmailChangeConfirmationAsync"/> correctly
    /// coordinates template processing and delivery for email change requests.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendEmailChangeConfirmationAsync_Should_CallProviderWithCorrectTemplate()
    {
        // Act
        await this._emailService.SendEmailChangeConfirmationAsync(Email, UserName, Link, CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(
            x => x.GetEmailTemplateAsync(
                this._templates.EmailChange,
                It.Is<Dictionary<string, string>>(
                    dict =>
                    dict["USER_NAME"] == UserName &&
                    dict["CHANGE_LINK"] == Link)),
            Times.Once);

        this._senderMock.Verify(
            x => x.SendAsync(
                Email,
                this._subjects.EmailChangeConfirmation,
                ExpectedBody,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendPasswordResetEmailAsync"/> correctly
    /// coordinates template processing and delivery for password reset requests.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendPasswordResetEmailAsync_Should_CallProviderAndSender()
    {
        // Act
        await this._emailService.SendPasswordResetEmailAsync(Email, UserName, Link, CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(
            x => x.GetEmailTemplateAsync(
                this._templates.PasswordReset,
                It.Is<Dictionary<string, string>>(
                    dict =>
                    dict["USER_NAME"] == UserName &&
                    dict["RESET_LINK"] == Link)),
            Times.Once);

        this._senderMock.Verify(
            x => x.SendAsync(
                Email,
                this._subjects.PasswordReset,
                ExpectedBody,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendEmailChangeSecurityAlertAsync"/> correctly
    /// coordinates template processing and delivery when an email change is requested.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendEmailChangeSecurityAlertAsync_Should_CallTemplateProviderAndSender_WithCorrectData()
    {
        // Arrange
        const string NewEmail = "new@example.com";

        // Act
        await this._emailService.SendEmailChangeSecurityAlertAsync(Email, NewEmail, UserName, Link, CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(
            x => x.GetEmailTemplateAsync(
                this._templates.EmailChangeSecurityAlert,
                It.Is<Dictionary<string, string>>(
                    dict =>
                    dict["USER_NAME"] == UserName &&
                    dict["NEW_EMAIL"] == NewEmail &&
                    dict["REVERT_LINK"] == Link)),
            Times.Once);

        this._senderMock.Verify(
            x => x.SendAsync(
                Email,
                this._subjects.EmailChangeSecurityAlert,
                ExpectedBody,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendAccountDeletedEmailAsync"/> correctly
    /// coordinates template processing and delivery when an account deleted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendAccountDeletedEmailAsync_Should_CallProviderAndSender()
    {
        // Act
        await this._emailService.SendAccountDeletedEmailAsync(Email, UserName, CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(
            x => x.GetEmailTemplateAsync(
                this._templates.AccountDeleted,
                It.Is<Dictionary<string, string>>(
                    dict =>
                    dict["USER_NAME"] == UserName)),
            Times.Once);
        this._senderMock.Verify(
            x => x.SendAsync(
                Email,
                this._subjects.AccountDeleted,
                ExpectedBody,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService"/> correctly propagates a <see cref="FileNotFoundException"/>
    /// if the requested email template does not exist on the file system.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendEmail_Should_PropagateException_WhenTemplateNotFound()
    {
        // Arrange
        this._templateProviderMock
            .Setup(x => x.GetEmailTemplateAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new FileNotFoundException("Template missing"));

        // Act
        Func<Task> act = () => this._emailService.SendPasswordResetEmailAsync(Email, UserName, Link, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
        .WithMessage("Template missing");
    }
}
