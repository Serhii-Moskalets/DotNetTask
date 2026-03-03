using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Abstractions.Interfaces.Security;

/// <summary>
/// Defines a contract for generating JSON Web Tokens for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A string representing the JWT.</returns>
    string GenerateToken(UserEntity user);
}
