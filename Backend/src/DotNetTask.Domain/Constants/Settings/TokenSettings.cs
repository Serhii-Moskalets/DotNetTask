namespace DotNetTask.Domain.Constants.Settings;

/// <summary>
/// Represents configuration settings for security tokens.
/// </summary>
public class TokenSettings
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string SectionName = "TokenSettings";

    /// <summary>
    /// Gets or sets the duration for which the email verification token remains valid.
    /// </summary>
    public TimeSpan EmailVerificationTokenDuration { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets or sets the duration for which the password reset token remains valid.
    /// </summary>
    public TimeSpan PasswordResetTokenDuration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the duration for which the email change token remains valid.
    /// </summary>
    public TimeSpan EmailChangeTokenDuration { get; set; } = TimeSpan.FromMinutes(15);
}
