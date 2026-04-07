using DotNetTask.Infrastructure.Notifications.Settings;
using DotNetTask.Infrastructure.Services;

using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetTask.Infrastructure.Test.Services;

/// <summary>
/// Contains unit tests for the <see cref="UrlProvider"/> class to ensure correct URL generation and parameter encoding.
/// </summary>
public class UrlProviderTests
{
    private const string BaseUrl = "https://todoList.com";
    private const string Url1 = "http://localhost:3000";
    private const string Url2 = "https://todolist.com";

    private const string ConfirmEmailPath = "confirm-email";
    private const string EmailChangeConfirmationPath = "confirm-email-change";
    private const string PasswordResetPath = "reset-password";
    private const string RevertEmailChangePath = "revert-email-change";

    private const string Token = "Security-token";
    private const string TokenWithSpecialCharacters = "abc/123+def==";

    private readonly Mock<IOptions<FrontendSettings>> _optionsMock;
    private readonly FrontendSettings _settings;
    private readonly UrlProvider _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlProviderTests"/> class.
    /// </summary>
    public UrlProviderTests()
    {
        this._optionsMock = new Mock<IOptions<FrontendSettings>>();

        this._settings = CreateFrontendSettings();

        this._optionsMock.Setup(x => x.Value).Returns(this._settings);

        this._sut = new UrlProvider(this._optionsMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailConfirmationLink"/> correctly handles different protocols
    /// (HTTP and HTTPS) provided in the configuration.
    /// </summary>
    /// <param name="baseUrl">The base URL string to be tested.</param>
    [Theory]
    [InlineData(Url1)]
    [InlineData(Url2)]
    public void GetEmailConfirmationLink_ShouldWorkWithAnyProtocol(string baseUrl)
    {
        // Arrange
        this._optionsMock.Setup(x => x.Value).Returns(CreateFrontendSettings(baseUrl: baseUrl));

        UrlProvider sut = new(this._optionsMock.Object);

        // Act
        string result = sut.GetEmailConfirmationLink(Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain(this._settings.ConfirmEmailPath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailConfirmationLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailConfirmationLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        string result = this._sut.GetEmailConfirmationLink(TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain(this._settings.ConfirmEmailPath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailChangeLink"/> correctly handles different protocols
    /// (HTTP and HTTPS) provided in the configuration.
    /// </summary>
    /// <param name="baseUrl">The base URL string to be tested.</param>
    [Theory]
    [InlineData(Url1)]
    [InlineData(Url2)]
    public void GetEmailChangeLink_ShouldWorkWithAnyProtocol(string baseUrl)
    {
        // Arrange
        this._optionsMock.Setup(x => x.Value).Returns(CreateFrontendSettings(baseUrl: baseUrl));

        UrlProvider sut = new(this._optionsMock.Object);

        // Act
        string result = sut.GetEmailChangeLink(Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain(this._settings.EmailChangeConfirmationPath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailChangeLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailChangeLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        string result = this._sut.GetEmailChangeLink(TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain(this._settings.EmailChangeConfirmationPath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailRevertLink"/> correctly handles different protocols
    /// (HTTP and HTTPS) provided in the configuration.
    /// </summary>
    /// <param name="baseUrl">The base URL string to be tested.</param>
    [Theory]
    [InlineData(Url1)]
    [InlineData(Url2)]
    public void GetEmailRevertLink_ShouldWorkWithAnyProtocol(string baseUrl)
    {
        // Arrange
        this._optionsMock.Setup(x => x.Value).Returns(CreateFrontendSettings(baseUrl: baseUrl));

        UrlProvider sut = new(this._optionsMock.Object);

        // Act
        string result = sut.GetEmailRevertLink(Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain(this._settings.RevertEmailChangePath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailRevertLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailRevertLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        string result = this._sut.GetEmailRevertLink(TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain(this._settings.RevertEmailChangePath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetPasswordResetLink(string)"/> correctly handles different protocols
    /// (HTTP and HTTPS) provided in the configuration.
    /// </summary>
    /// <param name="baseUrl">The base URL string to be tested.</param>
    [Theory]
    [InlineData(Url1)]
    [InlineData(Url2)]
    public void GetPasswordResetLink_ShouldWorkWithAnyProtocol(string baseUrl)
    {
        // Arrange
        this._optionsMock.Setup(x => x.Value).Returns(CreateFrontendSettings(baseUrl: baseUrl));

        UrlProvider sut = new(this._optionsMock.Object);

        // Act
        string result = sut.GetPasswordResetLink(Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain(this._settings.PasswordResetPath);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetPasswordResetLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetPasswordResetLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        string result = this._sut.GetPasswordResetLink(TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain(this._settings.PasswordResetPath);
    }

    /// <summary>
    /// Verifies that the <see cref="UrlProvider"/> correctly handles a trailing slash
    /// in the base URL configuration, ensuring that the generated link does not contain double slashes.
    /// </summary>
    [Fact]
    public void BuildUrl_ShouldHandleTrailingSlashInConfiguration()
    {
        // Arrange
        const string baseUrlWithSlash = "https://site.com/";
        this._optionsMock.Setup(x => x.Value).Returns(CreateFrontendSettings(baseUrl: baseUrlWithSlash));

        UrlProvider sut = new(this._optionsMock.Object);

        // Act
        string result = sut.GetEmailConfirmationLink(Token);

        // Assert
        result.Should().Contain("site.com/confirm-email");
        result.Should().NotContain("site.com//confirm-email");
    }

    /// <summary>
    /// Verifies that the <see cref="UrlProvider"/> constructor throws an <see cref="InvalidOperationException"/>
    /// when the required 'FrontendSettings:BaseUrl' configuration is missing.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrowException_WhenBaseUrlIsMissing()
    {
        // Arrange
        this._optionsMock.Setup(x => x.Value).Returns(new FrontendSettings
        {
            BaseUrl = null!,
        });

        // Act
        Action act = () => _ = new UrlProvider(this._optionsMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Frontend BaseUrl is not configured.");
    }

    private static FrontendSettings CreateFrontendSettings(
        string? baseUrl = null,
        string? confirmEmailPath = null,
        string? emailChangeConfirmationPath = null,
        string? passwordResetPath = null,
        string? revertEmailChangePath = null)
    {
        return new FrontendSettings
        {
            BaseUrl = baseUrl ?? BaseUrl,
            ConfirmEmailPath = confirmEmailPath ?? ConfirmEmailPath,
            EmailChangeConfirmationPath = emailChangeConfirmationPath ?? EmailChangeConfirmationPath,
            PasswordResetPath = passwordResetPath ?? PasswordResetPath,
            RevertEmailChangePath = revertEmailChangePath ?? RevertEmailChangePath,
        };
    }
}
