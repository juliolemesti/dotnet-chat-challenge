using Xunit;
using ChatChallenge.Api.Services;

namespace ChatChallenge.Tests;

public class PasswordServiceTests
{
  private readonly PasswordService _passwordService;

  public PasswordServiceTests()
  {
    _passwordService = new PasswordService();
  }

  [Fact]
  public void HashPassword_ShouldReturnValidHash()
  {
    var password = "TestPassword123";

    var hash = _passwordService.HashPassword(password);

    Assert.NotNull(hash);
    Assert.NotEmpty(hash);
    Assert.NotEqual(password, hash);
  }

  [Fact]
  public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
  {
    var password = "TestPassword123";
    var hash = _passwordService.HashPassword(password);

    var result = _passwordService.VerifyPassword(password, hash);

    Assert.True(result);
  }

  [Fact]
  public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
  {
    var password = "TestPassword123";
    var wrongPassword = "WrongPassword123";
    var hash = _passwordService.HashPassword(password);

    var result = _passwordService.VerifyPassword(wrongPassword, hash);

    Assert.False(result);
  }

  [Fact]
  public void HashPassword_WithSamePassword_ShouldGenerateDifferentHashes()
  {
    var password = "TestPassword123";

    var hash1 = _passwordService.HashPassword(password);
    var hash2 = _passwordService.HashPassword(password);

    Assert.NotEqual(hash1, hash2); // Should be different due to random salt
    
    // But both should verify correctly
    Assert.True(_passwordService.VerifyPassword(password, hash1));
    Assert.True(_passwordService.VerifyPassword(password, hash2));
  }

  [Fact]
  public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
  {
    var password = "TestPassword123";
    var invalidHash = "invalid-hash";

    var result = _passwordService.VerifyPassword(password, invalidHash);

    Assert.False(result);
  }
}
