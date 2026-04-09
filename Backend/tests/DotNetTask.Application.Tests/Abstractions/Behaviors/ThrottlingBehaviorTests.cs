using DotNetTask.Application.Abstractions.Behaviors;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Options;
using DotNetTask.Domain.Constants.Policy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Tests.Abstractions.Behaviors;

/// <summary>
/// Unit tests for the <see cref="ThrottlingBehavior{TRequest, TResponse}"/> class.
/// </summary>
/// <remarks>
/// These tests verify that the pipeline behavior correctly interacts with the
/// <see cref="IFloodProtectionService"/> to either allow or block request execution.
/// </remarks>
public class ThrottlingBehaviorTests
{
    private const string Identity = "user-1";
    private const string ActionName = "Login";
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private readonly Mock<IFloodProtectionService> _floodServiceMock;
    private readonly IOptions<ThrottlingSettings> _options;
    private readonly ThrottlingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingBehaviorTests"/> class
    /// and configures the mocked flood protection service.
    /// </summary>
    public ThrottlingBehaviorTests()
    {
        this._floodServiceMock = new Mock<IFloodProtectionService>();
        this._settings = new ThrottlingSettings
        {
            Actions = new Dictionary<string, ThrottlingActionSettings>
            {
                {
                    ActionName, new ThrottlingActionSettings
                    {
                        ActionName = ActionName,
                        MaxAttempts = MaxAttempts,
                        Window = Window,
                    }
                },
            },
        };

        Mock<IOptions<ThrottlingSettings>> optionsMock = new();
        optionsMock.Setup(o => o.Value).Returns(this._settings);
        this._options = optionsMock.Object;

    }

    /// <summary>
    /// Verifies that the behavior calls the next delegate in the pipeline when
    /// the flood protection service allows the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WhenAllowed_ShouldCallNext()
    {
        // Arrange
        IThrottledRequest request = CreateMockRequest();

        this._floodServiceMock
            .Setup(s => s.IsAllowedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);

        Result<Unit> expectedResponse = Result<Unit>.Success(Unit.Value);

        Task<Result<Unit>> Next(CancellationToken _) => Task.FromResult(expectedResponse);

        ThrottlingBehavior<IThrottledRequest, Result<Unit>> behavior = this.CreateBehavior<Result<Unit>>();

        // Act
        Result<Unit> result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        this._floodServiceMock.Verify(
            x => x.IsAllowedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that the behavior short-circuits the pipeline and returns a failure result
    /// when the flood protection service denies the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WhenNotAllowed_AndResponseIsResultBool_ShouldReturnFailure()
    {
        // Arrange
        Mock<IThrottledRequest> requestMock = new();
        requestMock.Setup(r => r.GetIdentity()).Returns(Identity);
        requestMock.Setup(r => r.ActionName).Returns(ActionName);

        this._floodServiceMock
            .Setup(x => x.IsAllowedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);

        static Task<Result<Unit>> Next(CancellationToken _) => throw new Exception("Should not be called");

        ThrottlingBehavior<IThrottledRequest, Result<Unit>> behavior = this.CreateBehavior<Result<Unit>>();

        // Act
        Result<Unit> result = await behavior.Handle(requestMock.Object, Next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be(ThrottlingPolicy.TooManyAttemptsMessage);
    }

    /// <summary>
    /// Verifies that the behavior correctly handles generic <see cref="Result{T}"/> responses
    /// by returning a failure result of the expected type when throttled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WhenNotAllowed_AndResponseIsGenericResult_ShouldReturnFailureOfCorrectType()
    {
        // Arrange
        IThrottledRequest request = CreateMockRequest();

        this._floodServiceMock
            .Setup(x => x.IsAllowedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);

        ThrottlingBehavior<IThrottledRequest, Result<TestDto>> behavior = this.CreateBehavior<Result<TestDto>>();

        // Act
        Result<TestDto> result = await behavior.Handle(request, _ => throw new Exception("Should not be called"), CancellationToken.None);

        // Assert
        result.Should().BeOfType<Result<TestDto>>();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be(ThrottlingPolicy.TooManyAttemptsMessage);
    }

    /// <summary>
    /// Verifies that an <see cref="InvalidOperationException"/> is thrown if the request
    /// is blocked but the expected response type is not compatible with <see cref="Result{T}"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Hanlde_WhenNotAllowed_AndResponseIsIncompatibleType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        IThrottledRequest request = CreateMockRequest();

        this._floodServiceMock
            .Setup(x => x.IsAllowedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);

        static Task<string> Next(CancellationToken _) => throw new Exception("Shopuld not be called");

        ThrottlingBehavior<IThrottledRequest, string> behavior = this.CreateBehavior<string>();

        // Act
        Func<Task<string>> result = async () => await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>().WithMessage(ThrottlingPolicy.InvalidOperationMessage);
    }

    /// <summary>
    /// Verifies that the behavior applies global default limits when a specific
    /// configuration for the given <see cref="IThrottledRequest.ActionName"/> is missing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_WhenActionSettingsMissing_ShouldIgnoreThrottlingAndCallNext()
    {
        // Arrange
        IThrottledRequest request = CreateMockRequest(identity: "test-user", actionName: "UnknownAction");

        this._floodServiceMock
            .Setup(x => x.IsAllowedAsync(
                "test-user",
                "UnknownAction",
                this._settings.DefaultLimit.MaxAttempts,
                this._settings.DefaultLimit.Window))
            .ReturnsAsync(true);

        bool nextCalled = false;
        Task<Result<Unit>> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(Result<Unit>.Success(Unit.Value));
        }

        ThrottlingBehavior<IThrottledRequest, Result<Unit>> behavior = this.CreateBehavior<Result<Unit>>();

        // Act
        await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        this._floodServiceMock.Verify(
            x => x.IsAllowedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    private static IThrottledRequest CreateMockRequest(string? identity = null, string? actionName = null)
    {
        Mock<IThrottledRequest> requestMock = new();
        requestMock.Setup(r => r.GetIdentity()).Returns(identity ?? Identity);
        requestMock.Setup(r => r.ActionName).Returns(actionName ?? ActionName);
        return requestMock.Object;
    }

    /// <summary>
    /// Helper method to instantiate the behavior with mocked dependencies.
    /// </summary>
    private ThrottlingBehavior<IThrottledRequest, TResponse> CreateBehavior<TResponse>() => new(this._floodServiceMock.Object, this._options);

    /// <summary>
    /// A simple Data Transfer Object used for testing generic response handling.
    /// </summary>
    private class TestDto { public string Name { get; set; } = string.Empty; }
}
