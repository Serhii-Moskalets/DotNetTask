using System.Security.Claims;
using DotNetTask.Api.Middleware;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Infrastructure.Web.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetTask.Api.Tests.Middleware;

/// <summary>
/// Unit tests for the <see cref="UserSecurityMiddleware"/> to ensure security constraints are enforced.
/// </summary>
public class UserSecurityMiddlewareTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly ApiEndpointOptions _endpointSettings;
    private readonly UserSecurityMiddleware _middleware;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSecurityMiddlewareTests"/> class.
    /// </summary>
    public UserSecurityMiddlewareTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._nextMock = new Mock<RequestDelegate>();

        ApiEndpointOptions apiEndpointOptions = new()
        {
            ResendEmailVerificationEndpoint = "/api/users/resend-email-verification",
            ResetPasswordEndpoint = "/api/auth/reset-password",
        };

        IOptions<ApiEndpointOptions> endpointOptions = Options.Create(apiEndpointOptions);

        this._endpointSettings = endpointOptions.Value;

        this._middleware = new UserSecurityMiddleware(
            this._nextMock.Object,
            endpointOptions);
    }

    /// <summary>
    /// Verifies that the middleware calls the next delegate when the user is not authenticated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_UserNotAuthenticated()
    {
        // Arrange
        DefaultHttpContext context = new()
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
        DefaultHttpContext context = CreateAuthenticatedContext(userId: null, stamp: null);

        // Act & Assert
        Func<Task> act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
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
        Guid userId = Guid.NewGuid();
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), "some-stamp");

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, bool, bool)?)null);

        // Act & Assert
        Func<Task> act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
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
        Guid userId = Guid.NewGuid();
        string tokenStamp = "old-stamp";
        string dbStamp = "new-stamp";
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), tokenStamp);

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((dbStamp, false, true));

        // Act & Assert
        Func<Task> act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
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
        Guid userId = Guid.NewGuid();
        string stamp = "valid-stamp";
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), stamp);
        context.Request.Path = "/api/todo/items";

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, true, true));

        // Act & Assert
        Func<Task> act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
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
        Guid userId = Guid.NewGuid();
        string stamp = "valid-stamp";
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), stamp);
        context.Request.Path = this._endpointSettings.ResetPasswordEndpoint;

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, true, true));

        // Act
        await this._middleware.InvokeAsync(context, this._uowMock.Object);

        // Assert
        this._nextMock.Verify(next => next(context), Times.Once);
    }

    /// <summary>
    /// Verifies that an <see cref="EmailResendVerificationException"/> is thrown when email is not confirmed and the user is not accessing the resend verification endpoint.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_ThrowResendEmailChangeVerification_When_EmailConfirmedIdFalse_And_NotResendEmailVerificationEndpoint()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string stamp = "valid-stamp";
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), stamp);
        context.Request.Path = "/api/todo/items";

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, true, false));

        // Act & Assert
        Func<Task> act = () => this._middleware.InvokeAsync(context, this._uowMock.Object);
        await act.Should().ThrowAsync<EmailResendVerificationException>();
    }

    /// <summary>
    /// Verifies that the middleware proceeds when the email is not confirmed but the user is accessing the resend verification endpoint.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task InvokeAsync_Should_CallNext_When_EmailConfirmedIdFalse_But_IsResendEmailVerificationEndpoint()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string stamp = "valid-stamp";
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), stamp);
        context.Request.Path = this._endpointSettings.ResendEmailVerificationEndpoint;

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, false, false));

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
        Guid userId = Guid.NewGuid();
        string stamp = "valid-stamp";
        DefaultHttpContext context = CreateAuthenticatedContext(userId.ToString(), stamp);

        this._uowMock.Setup(u => u.Users.GetUsersSecurityInfoAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((stamp, false, true));

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
        List<Claim> claims = [];
        if (userId != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        if (stamp != null)
        {
            claims.Add(new Claim(CustomClaims.SecurityStamp, stamp));
        }

        ClaimsIdentity identity = new(claims, "TestAuth");
        ClaimsPrincipal user = new(identity);

        return new DefaultHttpContext { User = user };
    }
}
