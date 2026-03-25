using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Infrastructure.Notifications.Services;

using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Notifications;

/// <summary>
/// Contains unit tests for the <see cref="EmailTemplateProvider"/> class,
/// ensuring correct email template loading and placeholder replacement behavior.
/// </summary>
public class EmailTemplateProviderTests
{
    private readonly EmailTemplateProvider _provider = new();

    /// <summary>
    /// Verifies that <see cref="EmailTemplateProvider.GetEmailTemplateAsync(string, Dictionary{string, string})"/>
    /// correctly replaces all placeholders in the email template with provided values.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test execution.
    /// </returns>
    [Fact]
    public async Task GetEmailTemplateAsync_ShouldReplaceAllPlaceholders()
    {
        // Arrange
        Dictionary<string, string> placeholders = new Dictionary<string, string>
        {
            { "USER_NAME", "john" },
            { "VERIFY_LINK", "https://todo-app.com/verify?token=abc" },
        };

        // Act
        string htmlResult = await this._provider.GetEmailTemplateAsync(EmailTemplates.EmailVerification, placeholders);

        // Assert
        htmlResult.Should().NotBeNullOrEmpty();

        htmlResult.Should().Contain("john");
        htmlResult.Should().Contain("https://todo-app.com/verify?token=abc");

        htmlResult.Should().NotContain("{{USER_NAME}}");
        htmlResult.Should().NotContain("{{VERIFY_LINK}}");
    }

    /// <summary>
    /// Verifies that <see cref="EmailTemplateProvider.GetEmailTemplateAsync(string, Dictionary{string, string})"/>
    /// throws a <see cref="FileNotFoundException"/> when the specified email template file does not exist.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test execution.
    /// </returns>
    [Fact]
    public async Task GetEmailTemplateAsync_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        // Arrange
        const string wrongTemplateName = "non_existent_template";

        // Act
        Func<Task<string>> act = async () => await this._provider.GetEmailTemplateAsync(wrongTemplateName, []);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*Email template not found at*");
    }

    /// <summary>
    /// Verifies that the provider correctly HTML-encodes placeholder values to prevent XSS injections
    /// when rendering the email body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetEmailTemplateAsync_ShouldHtmlEncodePlaceholderValues()
    {
        // Arrange
        Dictionary<string, string> placeholders = new Dictionary<string, string>
        {
            { "USER_NAME", "<script>alert('xss')</script>" },
            { "VERIFY_LINK", "http://test.com" },
        };

        // Act
        string result = await this._provider.GetEmailTemplateAsync(EmailTemplates.EmailVerification, placeholders);

        // Assert
        result.Should().Contain("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");
        result.Should().NotContain("<script>");
    }
}
