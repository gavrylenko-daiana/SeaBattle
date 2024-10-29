using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SeaBattle.Application.Interfaces;

namespace SeaBattle.API.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IJwtTokenService jwtTokenService, IAppUserService userService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            var userIdResult = jwtTokenService.GetUserIdFromToken(token);

            if (!userIdResult.IsFailure)
            {
                var userResult = await userService.GetUserById(userIdResult.Value);

                if (!userResult.IsFailure)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, userResult.Value.UserName),
                        new Claim(ClaimTypes.Email, userResult.Value.Email),
                        new Claim(ClaimTypes.NameIdentifier, userResult.Value.AppUserId.ToString())
                    };
                
                    var identity = new ClaimsIdentity(claims, "jwt");
                    var principal = new ClaimsPrincipal(identity);

                    context.User = principal;
                }
            }
        }

        await _next(context);
    }
}