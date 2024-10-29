using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeaBattle.Application.Interfaces;

namespace SeaBattle.API.Controllers;

[Authorize]
public class InvitationController : BaseApiController
{
    private readonly IGameInvitationService _invitationService;

    public InvitationController(IGameInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000)
    {
        return HandleResult(await _invitationService.GetAll(pageNumber, pageSize));
    }
}