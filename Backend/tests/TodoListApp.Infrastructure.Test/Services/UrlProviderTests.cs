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
    private readonly Mock<IConfiguration> _configurationMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlProviderTests"/> class.
    /// </summary>
    public UrlProviderTests()
    {
        this._configurationMock = new Mock<IConfiguration>();

        this._configurationMock
            .Setup(x => x["FrontendSettings:BaseUrl"])
            .Returns(BaseUrl);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailConfirmationLink"/> correctly handles different protocols
    /// (HTTP and HTTPS) provided in the configuration.
    /// </summary>
    /// <param name="baseUrl">The base URL string to be tested.</param>
    [Theory]
    [InlineData("http://localhost:3000")]
    [InlineData("https://todolist.com")]
    public void GetEmailConfirmationLink_ShouldWorkWithAnyProtocol(string baseUrl)
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["FrontendSettings:BaseUrl"]).Returns(baseUrl);
        var sut = new UrlProvider(configMock.Object);

        // Act
        var result = sut.GetEmailConfirmationLink("test@test.com", "123");

        // Assert
        result.Should().StartWith(baseUrl);
    }

    /// <summary>
    /// Verifies that <see cref="UrlProvider.GetEmailConfirmationLink"/> properly encodes special characters
    /// within query parameters to ensure the generated URL is web-safe.
    /// </summary>
    [Fact]
    public void GetEmailConfirmationLink_ShouldEncodeSpecialCharacters()
    {
        // Arrange
        var sut = new UrlProvider(this._configurationMock.Object);
        const string email = "user+special@example.com";
        const string token = "abc/123+def==";

        // Act
        var result = sut.GetEmailConfirmationLink(email, token);

        // Assert
        result.Should().NotContain("+");
        result.Should().Contain("user%2bspecial");
    }
}
