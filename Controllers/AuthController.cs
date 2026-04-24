
using Microsoft.AspNetCore.Mvc;
using homework3.Services;
using homework3.Exceptions;

namespace homework3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICustomLogger _logger;

    public AuthController(IAuthService authService, ICustomLogger logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public class RegisterRequest { public string Username { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class LoginRequest { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try 
        {
            var authData = await _authService.RegisterAsync(request.Username, request.Email, request.Password);

            return Ok(new
            {
                id = authData.Id,
                username = authData.Username,
                token = authData.Token
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authData = await _authService.LoginAsync(request.Email, request.Password);

        if (authData == null)
        {
            return Unauthorized(new { message = "Неверная почта или пароль" });
        }

        return Ok(new 
        { 
            id = authData.Id, 
            username = authData.Username,
            token = authData.Token
        });
    }
}


