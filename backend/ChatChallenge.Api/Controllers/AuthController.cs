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
    // Simple validation - in production, use proper password hashing/verification
    var user = await _userRepository.GetUserByEmailAsync(request.Email);
    
    if (user == null)
    {
      // User doesn't exist - return generic error message (don't specify if email or password is wrong)
      return Unauthorized(new { message = "Invalid credentials" });
    }

    // For demo purposes, assume password validation always passes if user exists
    // In production, verify hashed password here
    var isValidPassword = await _userRepository.ValidateUserCredentialsAsync(request.Email, request.Password);
    
    if (!isValidPassword)
    {
      return Unauthorized(new { message = "Invalid credentials" });
    }

    return Ok(new LoginResponse
    {
      Success = true,
      User = user,
      Token = _jwtService.GenerateToken(user)
    });
  }

  [HttpPost("register")]
  public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
  {
    // Check if user already exists by email
    var existingUserByEmail = await _userRepository.GetUserByEmailAsync(request.Email);
    if (existingUserByEmail != null)
    {
      return BadRequest(new { message = "User already exists" });
    }

    // Check if username is already taken
    var existingUserByUserName = await _userRepository.GetUserByUserNameAsync(request.UserName);
    if (existingUserByUserName != null)
    {
      return BadRequest(new { message = "User already exists" });
    }

    // Create new user
    var newUser = new User
    {
      Email = request.Email,
      UserName = request.UserName
    };

    try
    {
      var savedUser = await _userRepository.CreateUserAsync(newUser);

      return Ok(new LoginResponse
      {
        Success = true,
        User = savedUser,
        Token = _jwtService.GenerateToken(savedUser)
      });
    }
    catch (Exception)
    {
      // In case of any database error during user creation
      return BadRequest(new { message = "Registration failed. Please try again." });
    }
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
