using System.IdentityModel.Tokens.Jwt;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Infrastructure.Security;
using DotNetTask.Infrastructure.Security.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetTask.Infrastructure.Test.Security;

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

        Mock<IOptions<JwtSettings>> optionsMock = new();
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
        UserEntity user = UserEntityFactory.Create();

        // Act
        string tokenString = this._sut.GenerateToken(user);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(tokenString);

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
        Func<string> act = () => this._sut.GenerateToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
