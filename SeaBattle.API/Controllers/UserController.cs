using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeaBattle.Application.Interfaces;

namespace SeaBattle.API.Controllers;

[Authorize]
public class UserController : BaseApiController
{
    private readonly IAppUserService _userService;

    public UserController(IAppUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000)
    {
        return HandleResult(await _userService.GetAllUsers(pageNumber, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return HandleResult(await _userService.GetUserById(id));
    }
}