using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SeaBattle.API.Hubs.Interfaces;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;

namespace SeaBattle.API.Controllers;

[Authorize]
public class GameController : BaseApiController
{
    private readonly IGameService _gameService;
    private readonly IHubService _playHubService;

    public GameController(IGameService gameService, IHubService playHubService)
    {
        _gameService = gameService;
        _playHubService = playHubService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000)
    {
        return HandleResult(await _gameService.GetAll(pageNumber: pageNumber, pageSize: pageSize));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return HandleResult(await _gameService.GetById(id));
    }

    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] GameDto gameDto)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }
        
        gameDto.CreatorId = userId;
        
        var result = await _gameService.Insert(gameDto);
        
        await _playHubService.UpdateGameList();

        return result.IsFailure
            ? NotFound()
            : CreatedAtAction("GetById", new { id = result.Value.GameId }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GameDto gameDto)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }

        gameDto.CreatorId = userId;
        
        var result = await _gameService.Update(id, gameDto);
        
        await _playHubService.UpdateGameList();

        return result.IsFailure ? NotFound() : Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _gameService.Delete(id);

        await _playHubService.UpdateGameList();

        return result.IsFailure ? NotFound() : Ok();
    }

    [HttpPost("join/{id}")]
    public async Task<IActionResult> JoinUserToGame(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }

        var result = await _gameService.Join(id, userId);
        
        await _playHubService.UpdateGameList();

        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }
    
    [HttpPost("{gameId}/invite/{userId}")]
    public async Task<IActionResult> CreateInvitationForUser(GameInvitationDto gameInvitation)
    {
        var result = await _gameService.InviteUser(gameInvitation.GameId, gameInvitation.UserId);
        
        await _playHubService.UpdateGameList();
        
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }
    
    [HttpPost("accept/{gameId}")]
    public async Task<IActionResult> AcceptInvitation(int gameId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }
        
        var result = await _gameService.AcceptInvitation(gameId, userId);
        
        await _playHubService.UpdateGameList();
        
        return result.IsFailure ? BadRequest(result.Error) : Ok(result.Value);
    }
    
    [HttpPut("add/{id}")]
    public async Task<IActionResult> AddShipToField([FromBody] ShipDto shipDto)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }
        
        var result = await _gameService.AddShipToField(shipDto, userId);

        return result.IsFailure ? NotFound() : Ok();
    }

    [HttpPatch("ready/{gameId}")]
    public async Task<IActionResult> SetPlayerStatusGameAsReady(int gameId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }
        
        var result = await _gameService.UpdateUserStatusOnReady(gameId, userId);
        
        await _playHubService.CoordinateUpdated(gameId);

        return result.IsFailure ? NotFound() : Ok();
    }

    [HttpPut("type/{coordinateId}/{gameId}")]
    public async Task<IActionResult> SetNewCoordinateType(int coordinateId, int gameId)
    {
        var result = await _gameService.UpdateCoordinateType(coordinateId);
        
        if (result.IsFailure)
        {
            return NotFound();
        }

        await _playHubService.CoordinateUpdated(gameId);
        
        return Ok();
    }

    [HttpPut("turn/{gameId}/{coordinateId}")]
    public async Task<IActionResult> UpdateTurn(int gameId, int coordinateId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest();
        }

        var result = await _gameService.UpdatePlayerTurn(gameId, coordinateId, userId);
        
        if (result.IsFailure)
        {
            return NotFound();
        }
        
        await _playHubService.CoordinateUpdated(gameId);
        
        return Ok();
    }
}