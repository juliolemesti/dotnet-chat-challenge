using ChatChallenge.Core.Entities;
using System.Security.Claims;

namespace ChatChallenge.Application.Interfaces;

/// <summary>
/// Service for handling JWT token operations
/// </summary>
public interface IJwtService
{
  /// <summary>
  /// Generate a JWT token for a user
  /// </summary>
  /// <param name="user">The user to generate token for</param>
  /// <returns>JWT token string</returns>
  string GenerateToken(User user);

  /// <summary>
  /// Validate a JWT token and extract claims
  /// </summary>
  /// <param name="token">The JWT token to validate</param>
  /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
  ClaimsPrincipal? ValidateToken(string token);
}
