using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models.Dto;

namespace SeaBattle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAppUserService _userService;
    private readonly IAuthenticationService _authenticationService;

    public AccountController(IAppUserService userService, IAuthenticationService authenticationService)
    {
        _userService = userService;
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _userService.CreateUser(registerDto);

        if (result.IsFailure)
        {
            return BadRequest(new { errors = result.Error.Message });
        }
        
        var userDto = _userService.GetUserDto(result.Value);

        return Ok(userDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var userResult = await _authenticationService.Authenticate(loginDto);

        return userResult.IsFailure ? Unauthorized(userResult.Error) : Ok(userResult.Value);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }
        
        var userResult = await _userService.GetUserById(userId);
        var userObject = _userService.GetUserDto(userResult.Value);
            
        return userObject is null ? Unauthorized() : Ok(userObject);
    }
}