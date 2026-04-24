using MicrobloggingApp.API.DTOs;
using MicrobloggingApp.API.Helpers;
using MicrobloggingApp.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly JwtTokenHelper _jwtTokenHelper;

    public LoginController(IUserService userService)
    {
        _userService = userService;
        _jwtTokenHelper = new JwtTokenHelper(
            "YourSecretKey", // Secret key from configuration
            "YourIssuer",    // Issuer from configuration
            "YourAudience"   // Audience from configuration
        );
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (_userService.ValidateUser(request.Username, request.Password))
        {
            var token = _jwtTokenHelper.GenerateToken(request.Username);
            return Ok(new { Token = token });
        }

        return Unauthorized("Invalid username or password");
    }
}
