using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using ChatChallenge.Api.Services;
using ChatChallenge.Core.Entities;

namespace ChatChallenge.Tests;

public class JwtServiceTests
{
    private readonly IJwtService _jwtService;

    public JwtServiceTests()
    {
        // Create a mock configuration for testing
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "ThisIsATestSecretKeyForJWTTokenGenerationThatIsLongEnough"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiryInMinutes", "60"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User 
        { 
            Id = 1, 
            UserName = "testuser", 
            Email = "test@example.com" 
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = new User 
        { 
            Id = 1, 
            UserName = "testuser", 
            Email = "test@example.com" 
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal("testuser", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("test@example.com", principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal("1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }
}