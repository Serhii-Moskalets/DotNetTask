using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TodoListApp.Api.Middleware;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Constants.Settings;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Api.Tests.Middleware;

/// <summary>
/// Unit tests for the <see cref="UserSecurityMiddleware"/> to ensure security constraints are enforced.
/// </summary>
public class UserSecurityMiddlewareTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly AuthSettings _authSettings;
    private readonly UserSecurityMiddleware _middleware;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSecurityMiddlewareTests"/> class.
    /// </summary>
    public UserSecurityMiddlewareTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._nextMock = new Mock<RequestDelegate>();
        this._authSettings = new AuthSettings();
        this._middleware = new UserSecurityMiddleware(this._nextMock.Object, this._authSettings);
    }

    /// <summary>
    /// Verifies that the middleware calls the next delegate when the user is not authenticated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_UserNotAuthenticated()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()),
        };

        // Act
        await this._middleware.InvokeAsync(context, this._uowMock.Object);

        // Assert
        this._nextMock.Verify(next => next(context), Times.Once);
    }

    /// <summary>
    /// Verifies that an <see cref="UnauthorizedAccessException"/> is thrown when required claims are missing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_ThrowUnauthorized_When_ClaimsAreMissing()
    {
        // Arrange
        var context = CreateAuthenticatedContext(userId: null, stamp: null);

        // Act & Assert
        var act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(TokenPolicy.InvalidUserIdentityMessage);
    }

    /// <summary>
    /// Verifies that an <see cref="UnauthorizedAccessException"/> is thrown when the user is not found in the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_ThrowUnauthorized_When_UserNotFoundInDb()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(userId.ToString(), "some-stamp");

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, bool)?)null);

        // Act & Assert
        var act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(UserPolicy.AccountNotFoundMessage);
    }

    /// <summary>
    /// Verifies that an <see cref="UnauthorizedAccessException"/> is thrown when the security stamp in the token doesn't match the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_ThrowUnauthorized_When_SecurityStampMismatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenStamp = "old-stamp";
        var dbStamp = "new-stamp";
        var context = CreateAuthenticatedContext(userId.ToString(), tokenStamp);

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((dbStamp, false));

        // Act & Assert
        var act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(UserPolicy.SessionExpiredMessage);
    }

    /// <summary>
    /// Verifies that a <see cref="PasswordChangeRequiredException"/> is thrown when the user must change their password and is not accessing the reset endpoint.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_ThrowPasswordChangeRequired_When_MustChangeIsTrue_And_NotResetEndpoint()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stamp = "valid-stamp";
        var context = CreateAuthenticatedContext(userId.ToString(), stamp);
        context.Request.Path = "/api/todo/items";

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, true));

        // Act & Assert
        var act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
        await act.Should().ThrowAsync<PasswordChangeRequiredException>();
    }

    /// <summary>
    /// Verifies that the middleware proceeds when a password change is required but the user is accessing the password reset endpoint.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_MustChangeIsTrue_But_IsResetEndpoint()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stamp = "valid-stamp";
        var context = CreateAuthenticatedContext(userId.ToString(), stamp);
        context.Request.Path = this._authSettings.ResetPasswordEndpoint;

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, true));

        // Act
        await this._middleware.InvokeAsync(context, this._uowMock.Object);

        // Assert
        this._nextMock.Verify(next => next(context), Times.Once);
    }

    /// <summary>
    /// Verifies that the middleware proceeds normally when the user identity and security stamp are valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_Proceed_When_EverythingIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stamp = "valid-stamp";
        var context = CreateAuthenticatedContext(userId.ToString(), stamp);

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, false));

        // Act
        await this._middleware.InvokeAsync(context, this._uowMock.Object);

        // Assert
        this._nextMock.Verify(next => next(context), Times.Once);
    }

    /// <summary>
    /// Creates a mocked <see cref="DefaultHttpContext"/> with an authenticated user and specified claims.
    /// </summary>
    /// <param name="userId">The user identifier to be added to claims.</param>
    /// <param name="stamp">The security stamp to be added to claims.</param>
    /// <returns>A configured <see cref="DefaultHttpContext"/>.</returns>
    private static DefaultHttpContext CreateAuthenticatedContext(string? userId, string? stamp)
    {
        var claims = new List<Claim>();
        if (userId != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        if (stamp != null)
        {
            claims.Add(new Claim(CustomClaims.SecurityStamp, stamp));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        return new DefaultHttpContext { User = user };
    }
}
