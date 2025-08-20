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
        var user = new User 
        { 
            Id = 1, 
            UserName = "testuser", 
            Email = "test@example.com" 
        };

        var token = _jwtService.GenerateToken(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        var user = new User 
        { 
            Id = 1, 
            UserName = "testuser", 
            Email = "test@example.com" 
        };
        var token = _jwtService.GenerateToken(user);

        var principal = _jwtService.ValidateToken(token);

        Assert.NotNull(principal);
        Assert.Equal("testuser", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("test@example.com", principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal("1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        var invalidToken = "invalid.jwt.token";

        var principal = _jwtService.ValidateToken(invalidToken);

        Assert.Null(principal);
    }
}