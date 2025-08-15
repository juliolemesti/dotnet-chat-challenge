using Microsoft.AspNetCore.Mvc;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;
using ChatChallenge.Api.Services;

namespace ChatChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IUserRepository _userRepository;
  private readonly IJwtService _jwtService;

  public AuthController(IUserRepository userRepository, IJwtService jwtService)
  {
    _userRepository = userRepository;
    _jwtService = jwtService;
  }

  [HttpPost("login")]
  public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
  {
    // For demo purposes - simple authentication
    var isValid = await _userRepository.ValidateUserCredentialsAsync(request.Email, request.Password);
    
    if (!isValid)
    {
      // Try to create user if doesn't exist (for demo)
      var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
      if (existingUser == null)
      {
        var newUser = new User
        {
          Email = request.Email,
          UserName = request.Email.Split('@')[0] // Simple username from email
        };
        await _userRepository.CreateUserAsync(newUser);
        isValid = true;
      }
    }

    if (!isValid)
    {
      return Unauthorized(new { message = "Invalid credentials" });
    }

    var user = await _userRepository.GetUserByEmailAsync(request.Email);
    
    return Ok(new LoginResponse
    {
      Success = true,
      User = user!,
      Token = _jwtService.GenerateToken(user!)
    });
  }

  [HttpPost("register")]
  public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
  {
    var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
    if (existingUser != null)
    {
      return BadRequest(new { message = "User already exists" });
    }

    var newUser = new User
    {
      Email = request.Email,
      UserName = request.UserName
    };

    var savedUser = await _userRepository.CreateUserAsync(newUser);

    return Ok(new LoginResponse
    {
      Success = true,
      User = savedUser,
      Token = _jwtService.GenerateToken(savedUser)
    });
  }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string UserName, string Password);
public record LoginResponse
{
  public bool Success { get; set; }
  public User User { get; set; } = null!;
  public string Token { get; set; } = string.Empty;
}
