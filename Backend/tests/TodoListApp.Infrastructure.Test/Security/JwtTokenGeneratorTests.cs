using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Test.Common;
using TodoListApp.Infrastructure.Security;
using TodoListApp.Infrastructure.Security.Settings;

namespace TodoListApp.Infrastructure.Test.Security;

/// <summary>
/// Contains unit tests for the <see cref="JwtTokenGenerator"/> class.
/// </summary>
public class JwtTokenGeneratorTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtTokenGenerator _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGeneratorTests"/> class.
    /// Sets up a mocked configuration and the system under test (SUT).
    /// </summary>
    public JwtTokenGeneratorTests()
    {
        this._jwtSettings = new JwtSettings
            {
                Secret = "super-secret-key-at-least-32-characters-long",
                ExpiryMinutes = 60,
                Issuer = "TestIssuer",
                Audience = "TestAudience",
            };

        var optionsMock = new Mock<IOptions<JwtSettings>>();
        optionsMock.Setup(x => x.Value).Returns(this._jwtSettings);

        this._sut = new JwtTokenGenerator(optionsMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="JwtTokenGenerator.GenerateToken"/> returns a valid,
    /// properly formatted JWT containing the correct user claims and metadata.
    /// </summary>
    [Fact]
    public void GenerateToken_ShouldReturnValidJwt_WhenUserIsValid()
    {
        // Arrange
        var user = UserEntityFactory.Create();

        // Act
        var tokenString = this._sut.GenerateToken(user);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        jwtToken.Subject.Should().Be(user.Id.ToString());
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be(user.Email.Value);
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value.Should().Be(user.UserName.Value);

        jwtToken.Issuer.Should().Be(this._jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(this._jwtSettings.Audience);
    }

    /// <summary>
    /// Verifies that <see cref="JwtTokenGenerator.GenerateToken"/> throws an
    /// <see cref="ArgumentNullException"/> when the provided user entity is null.
    /// </summary>
    [Fact]
    public void GenerateToken_ShouldThrowException_WhenUserIsNull()
    {
        // Act
        var act = () => this._sut.GenerateToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
