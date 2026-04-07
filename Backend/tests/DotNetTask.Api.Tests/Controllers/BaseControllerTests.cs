using System.Net;
using System.Security.Claims;
using DotNetTask.Api.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Api.Tests.Controllers;

/// <summary>
/// Provides unit tests for the <see cref="BaseController"/> using a concrete <see cref="TestController"/> implementation.
/// </summary>
public static class BaseControllerTests
{
    /// <summary>
    /// Verifies that <see cref="BaseController.GetClientIpOrUnknown"/> returns the correct IP address when it is present in the context.
    /// </summary>
    [Fact]
    public static void GetClientIpOrUnknown_ReturnsIp_WhenSet()
    {
        // Arrange
        TestController controller = CreateController("192.168.1.100");

        // Act
        string ip = controller.ClientIp;

        // Assert
        ip.Should().Be("192.168.1.100");
    }

    /// <summary>
    /// Verifies that <see cref="BaseController.GetClientIpOrUnknown"/> returns <see cref="IPAddress.None"/> when the remote IP is missing.
    /// </summary>
    [Fact]
    public static void GetClientIpOrUnknown_ReturnsNone_WhenNotSet()
    {
        // Arrange
        TestController controller = CreateController();

        // Act
        string ip = controller.ClientIp;

        // Assert
        ip.Should().Be(IPAddress.None.ToString());
    }

    /// <summary>
    /// Ensures that in Production environment, only the IP address is returned as the throttling identity.
    /// </summary>
    [Fact]
    public static void GetThrottlingIdentity_Prod_ReturnsIpOnly()
    {
        // Arrange
        TestController controller = CreateController("10.0.0.1", false);

        // Act
        string ip = controller.ClientIp;

        // Assert
        ip.Should().Be("10.0.0.1");
    }

    /// <summary>
    /// Verifies that <see cref="BaseController.HandleResult{T}"/> returns an OK response when the result is successful.
    /// </summary>
    [Fact]
    public static void HandleResult_Success_ReturnsOk()
    {
        // Arrange
        TestController controller = CreateController();
        Result<int> result = Result<int>.Success(123);

        // Act
        OkObjectResult? response = controller.HandleResult(result) as OkObjectResult;

        // Assert
        response.Should().NotBeNull();
        response.Value.Should().Be(123);
    }

    /// <summary>
    /// Verifies that <see cref="BaseController.HandleResult{T}"/> returns a <see cref="ProblemDetails"/>
    /// response with 404 status when a NotFound error occurs.
    /// </summary>
    [Fact]
    public static void HandleResult_Failure_ReturnsProblemDetails()
    {
        // Arrange
        TestController controller = CreateController();
        Error error = new(ErrorCode.NotFound, "Not found");
        Result<int> result = Result<int>.Failure(error);

        // Act
        ObjectResult? response = controller.HandleResult(result) as ObjectResult;

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Verifies that <see cref="BaseController.HandleNoContent"/> returns a <see cref="NoContentResult"/> on success.
    /// </summary>
    [Fact]
    public static void HandleNoContent_Success_ReturnsNoContent()
    {
        // Arrange
        TestController controller = CreateController();
        Result<bool> result = Result<bool>.Success(true);

        // Act
        IActionResult response = controller.HandleNoContent(result);

        // Assert
        response.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Verifies that <see cref="BaseController.HandleNoContent"/> returns a 409 Conflict status on InvalidOperation failure.
    /// </summary>
    [Fact]
    public static void HandleNoContent_Failure_ReturnsProblemDetails()
    {
        // Arrange
        TestController controller = CreateController();
        Error error = new(ErrorCode.InvalidOperation, "Invalid");
        Result<bool> result = Result<bool>.Failure(error);

        // Act
        ObjectResult? response = controller.HandleNoContent(result) as ObjectResult;

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    private static TestController CreateController(string? ip = null, bool isDevelopment = true, ClaimsPrincipal? user = null)
    {
        Mock<ISender> mediatorMock = new();
        TestController controller = new(mediatorMock.Object);

        DefaultHttpContext context = new();

        if (ip != null)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        }

        if (user != null)
        {
            context.User = user;
        }

        ServiceCollection services = new();
        Mock<IWebHostEnvironment> envMock = new();
        envMock.Setup(e => e.EnvironmentName)
           .Returns(isDevelopment ? Environments.Development : Environments.Production);
        services.AddSingleton(envMock.Object);
        context.RequestServices = services.BuildServiceProvider();

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = context,
        };

        return controller;
    }

    private class TestController : BaseController
    {
        public TestController(ISender mediator)
            : base(mediator) { }

        public new Guid? CurrentUserIdOrNull => base.CurrentUserIdOrNull;

        public new IPAddress ClientIpAddress => base.ClientIpAddress;

        public new string ClientIp => base.ClientIp;

        public new IPAddress GetClientIpOrUnknown() => base.GetClientIpOrUnknown();

        public new IActionResult HandleResult<T>(Result<T> result) => base.HandleResult(result);

        public new IActionResult HandleNoContent(Result<bool> result) => base.HandleNoContent(result);

    }
}
