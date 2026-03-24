using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TodoListApp.Api.Middleware;
using TodoListApp.Api.Tests.Common;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Api.Tests.Middleware;

/// <summary>
/// Unit tests for the <see cref="GlobalExceptionHandler"/> exception handler.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerTests"/> class.
    /// </summary>
    public GlobalExceptionHandlerTests()
    {
        this._loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        this._handler = new GlobalExceptionHandler(this._loggerMock.Object);

    }

    /// <summary>
    /// Verifies that <see cref="DomainException"/> results in a 400 Bad Request response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_400_For_DomainException()
    {
        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new DomainException("Domain error");

        await this._handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.Title.Should().Be(DomainPolicy.BusinessRuleViolationMessage);
        response.Detail.Should().Be("Domain error");
    }

    /// <summary>
    /// Verifies that <see cref="PasswordChangeRequiredException"/> results in a 403 Forbidden response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_403_For_PasswordChangeRequiredException()
    {
        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new PasswordChangeRequiredException("Change password");

        await this._handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        response.Title.Should().Be(UserPolicy.MustChangePasswordMessage);
        response.Detail.Should().Be("Change password");
    }

    /// <summary>
    /// Verifies that <see cref="EmailResendVerificationException"/> results in a 403 Forbidden response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_403_For_EmailResendVerificationException()
    {
        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new EmailResendVerificationException();

        await this._handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        response.Title.Should().Be(UserPolicy.EmailIsNotConfirmedMessage);
        response.Detail.Should().Be(UserPolicy.EmailIsNotConfirmedMessage);
    }

    /// <summary>
    /// Verifies that <see cref="KeyNotFoundException"/> results in a 404 Not Found response and logs a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_404_And_LogWarning_For_KeyNotFoundException()
    {
        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new KeyNotFoundException();

        await this._handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.Title.Should().Be(DomainPolicy.ResourceNotFoundMessage);

        this._loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that an unhandled <see cref="Exception"/> results in a 500 Internal Server Error response and logs an error.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_500_And_LogError_For_UnknownException()
    {
        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new Exception("boom");

        await this._handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        response.Title.Should().Be(DomainPolicy.ServerErrorMessage);
        response.Detail.Should().Be(DomainPolicy.UnexpectedErrorMessage);

        this._loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="UnauthorizedAccessException"/> results in a 401 Unauthorized response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_401_For_UnauthorizedAccessException()
    {
        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new UnauthorizedAccessException("Unauthorized");

        await this._handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        response.Title.Should().Be(UserPolicy.SessionExpiredMessage);
        response.Detail.Should().Be("Unauthorized");
    }
}
