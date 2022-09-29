using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  private readonly IPasswordHasher<User> _passwordHasher;
  private readonly IConfiguration _configuration;
  public AuthController(IAuthService authService, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
  {
    _authService = authService;
    _passwordHasher = passwordHasher;
    _configuration = configuration;
  }


  [HttpPost("signup")]
  [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Signup(UserSignupDto userSignupDto)
  {
    try
    {
      var user = userSignupDto.toUser();
      var hashedPassword = _passwordHasher.HashPassword(user, user.Password);
      user.Password = hashedPassword;
      await _authService.CreateUser(user);
      return NoContent();
    }
    catch (ArgumentException)
    {
      return BadRequest();
    }
    catch (Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPost("login")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Signin(UserSigninDto userSigninDto)
  {
    try
    {
      var user = userSigninDto.toUser();
      if (user.Email == null)
      {
        throw new ArgumentException("Email is not provided");
      }
      var existingUser = await _authService.GetUserByEmail(user.Email);
      if (existingUser == null)
      {
        throw new ArgumentException("User is not existed");
      }
      var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password);
      if (passwordVerificationResult == PasswordVerificationResult.Failed)
      {
        throw new AuthenticationException("Password is not matched");
      }

      var token = GenerateJwtToken(existingUser.Id, existingUser.Username, existingUser.Email);
      return Ok(new { token = token });
    }
    catch (ArgumentException)
    {
      return BadRequest();
    }
    catch (System.Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpGet("get")]
  [Authorize]
  public IActionResult Get()
  {
    var username = User.FindFirstValue(ClaimTypes.Name);
    return Ok(username);
  }

  private string GenerateJwtToken(string uid, string username, string email)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, uid), new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Email, email) }),
      Expires = DateTime.UtcNow.AddMinutes(10),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }
}