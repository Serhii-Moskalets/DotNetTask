using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TodoListApp.Infrastructure.Services;

namespace TodoListApp.Infrastructure.Test.Services;

/// <summary>
/// Contains unit tests for the <see cref="UrlProvider"/> class to ensure correct URL generation and parameter encoding.
/// </summary>
public class UrlProviderTests
{
    private const string BaseUrl = "https://todoList.com";
    private const string Url1 = "http://localhost:3000";
    private const string Url2 = "https://todolist.com";

    private const string Token = "Security-token";
    private const string TokenWithSpecialCharacters = "abc/123+def==";

    private readonly Guid userId = Guid.NewGuid();

    private readonly Mock<IConfiguration> _configurationMock;
    private readonly UrlProvider _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlProviderTests"/> class.
    /// </summary>
    public UrlProviderTests()
    {
        this._configurationMock = new Mock<IConfiguration>();

        this._configurationMock
            .Setup(x => x["FrontendSettings:BaseUrl"])
            .Returns(BaseUrl);

        this._sut = new UrlProvider(this._configurationMock.Object);
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
        this._configurationMock.Setup(x => x["FrontendSettings:BaseUrl"]).Returns(baseUrl);
        var sut = new UrlProvider(this._configurationMock.Object);

        // Act
        var result = sut.GetEmailConfirmationLink(this.userId, Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain("/confirm-email");
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailConfirmationLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailConfirmationLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        var result = this._sut.GetEmailConfirmationLink(this.userId, TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain($"userId={this.userId}");
        result.Should().Contain("/confirm-email");
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
        this._configurationMock.Setup(x => x["FrontendSettings:BaseUrl"]).Returns(baseUrl);
        var sut = new UrlProvider(this._configurationMock.Object);

        const string NewEmail = "newexample@gmail.com";

        // Act
        var result = sut.GetEmailChangeLink(this.userId, Token, NewEmail);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain("/confirm-email-change");
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailChangeLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailChangeLink_ShouldEncodeSpecialCharacters()
    {
        // Arrange
        const string NewEmail = "newexample@gmail.com";

        // Act
        var result = this._sut.GetEmailChangeLink(this.userId, TokenWithSpecialCharacters, NewEmail);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain($"userId={this.userId}");
        result.Should().Contain("/confirm-email-change");
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
        this._configurationMock.Setup(x => x["FrontendSettings:BaseUrl"]).Returns(baseUrl);
        var sut = new UrlProvider(this._configurationMock.Object);

        // Act
        var result = sut.GetEmailRevertLink(this.userId, Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain("/revert-confirm-email");
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailRevertLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailRevertLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        var result = this._sut.GetEmailRevertLink(this.userId, TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain($"userId={this.userId}");
        result.Should().Contain("/revert-confirm-email");
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetPasswordResetLink(Guid, string)"/> correctly handles different protocols
    /// (HTTP and HTTPS) provided in the configuration.
    /// </summary>
    /// <param name="baseUrl">The base URL string to be tested.</param>
    [Theory]
    [InlineData(Url1)]
    [InlineData(Url2)]
    public void GetPasswordResetLink_ShouldWorkWithAnyProtocol(string baseUrl)
    {
        // Arrange
        this._configurationMock.Setup(x => x["FrontendSettings:BaseUrl"]).Returns(baseUrl);
        var sut = new UrlProvider(this._configurationMock.Object);

        // Act
        var result = sut.GetPasswordResetLink(this.userId, Token);

        // Assert
        result.Should().StartWith(baseUrl);
        result.Should().Contain("/reset-password");
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetPasswordResetLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetPasswordResetLink_ShouldEncodeSpecialCharacters()
    {
        // Act
        var result = this._sut.GetPasswordResetLink(this.userId, TokenWithSpecialCharacters);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("abc%2f123%2bdef%3d%3d");
        result.Should().Contain($"userId={this.userId}");
        result.Should().Contain("/reset-password");
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
        this._configurationMock.Setup(x => x["FrontendSettings:BaseUrl"]).Returns(baseUrlWithSlash);
        var sut = new UrlProvider(this._configurationMock.Object);

        // Act
        var result = sut.GetEmailConfirmationLink(this.userId, Token);

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
        var emptyConfig = new Mock<IConfiguration>();
        emptyConfig.Setup(x => x["FrontendSettings:BaseUrl"]).Returns((string?)null);

        // Act
        Action act = () => _ = new UrlProvider(emptyConfig.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Frontend BaseUrl is not configured.");
    }
}
