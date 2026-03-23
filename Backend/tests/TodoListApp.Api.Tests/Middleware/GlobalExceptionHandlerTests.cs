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
    /// <summary>
    /// Verifies that <see cref="DomainException"/> results in a 400 Bad Request response.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_400_For_DomainException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);

        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new DomainException("Domain error");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

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
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);

        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new PasswordChangeRequiredException("Change password");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        response.Title.Should().Be(UserPolicy.MustChangePasswordMessage);
        response.Detail.Should().Be("Change password");
    }

    /// <summary>
    /// Verifies that <see cref="KeyNotFoundException"/> results in a 404 Not Found response and logs a warning.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Should_Return_404_And_LogWarning_For_KeyNotFoundException()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);

        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new KeyNotFoundException();

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.Title.Should().Be(DomainPolicy.ResourceNotFoundMessage);

        loggerMock.Verify(
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
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);

        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new Exception("boom");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        response.Title.Should().Be(DomainPolicy.ServerErrorMessage);
        response.Detail.Should().Be(DomainPolicy.UnexpectedErrorMessage);

        loggerMock.Verify(
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
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);

        using var testContext = Setup.CreateHttpContext();
        var context = testContext.Context;

        var exception = new UnauthorizedAccessException("Unauthorized");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        var response = await context.ReadProblemDetailsAsync();

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        response.Title.Should().Be(UserPolicy.SessionExpiredMessage);
        response.Detail.Should().Be("Unauthorized");
    }
}
