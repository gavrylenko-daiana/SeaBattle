using Microsoft.AspNetCore.Mvc;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result is null)
        {
            return NotFound();
        }
        
        if (!result.IsFailure && result.Value is not null)
        {
            return Ok(result.Value);
        }

        if (!result.IsFailure && result.Value is null)
        {
            return NotFound();
        }

        return BadRequest(result.Error);
    }
}