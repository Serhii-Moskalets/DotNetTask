using DotNetTask.Application.Abstractions.Options;
using FluentAssertions;

namespace DotNetTask.Application.Tests.Abstractions.Options;

/// <summary>
/// Unit tests for the <see cref="ThrottlingSettings"/> class, focusing on dynamic property resolution.
/// </summary>
public class ThrottlingSettingsTests
{
    /// <summary>
    /// Verifies that <see cref="ThrottlingSettings.GetSettingsByAction"/> returns default settings with the provided action name
    /// when an action name is provided that does not match any public property in the class.
    /// </summary>
    [Fact]
    public void GetSettingsByAction_ShouldReturnDefaultSettings_WithPovidedActionName_WhenPropertyDoesNotExist()
    {
        // Arrange
        ThrottlingSettings settings = new();

        // Act
        ThrottlingActionSettings? result = settings.GetSettingsByAction("NonExistentProperty");

        // Assert
        result.Should().NotBeNull();
        result.ActionName.Should().Be("NonExistentProperty");
    }

    /// <summary>
    /// Verifies that <see cref="ThrottlingSettings.GetSettingsByAction"/> returns <see langword="null"/>
    /// when an action name is null or white space.
    /// </summary>
    /// <param name="actionName">The name of the action being tested.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetSettingsByAction_ShouldReturnNull_WhenActionNameIsNullOrWhiteSpace(string? actionName)
    {
        // Arrange
        ThrottlingSettings settings = new();

        // Act
        ThrottlingActionSettings? result = settings.GetSettingsByAction(actionName!);

        // Assert
        result.Should().BeNull();
    }
}
