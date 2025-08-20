using Microsoft.AspNetCore.Mvc;
using ChatChallenge.Core.Interfaces;
using ChatChallenge.Core.Entities;
using ChatChallenge.Application.Interfaces;

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
    var user = await _userRepository.GetUserByEmailAsync(request.Email);
    
    if (user == null)
    {
      return Unauthorized(new { message = "Invalid credentials" });
    }

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
    var existingUserByEmail = await _userRepository.GetUserByEmailAsync(request.Email);
    if (existingUserByEmail != null)
    {
      return BadRequest(new { message = "User already exists" });
    }

    var existingUserByUserName = await _userRepository.GetUserByUserNameAsync(request.UserName);
    if (existingUserByUserName != null)
    {
      return BadRequest(new { message = "User already exists" });
    }

    var newUser = new User
    {
      Email = request.Email,
      UserName = request.UserName
    };

    try
    {
      var savedUser = await _userRepository.CreateUserAsync(newUser, request.Password);

      return Ok(new LoginResponse
      {
        Success = true,
        User = savedUser,
        Token = _jwtService.GenerateToken(savedUser)
      });
    }
    catch (Exception)
    {
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
