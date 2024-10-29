using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models.Results;
using SeaBattle.Domain.Token;

namespace SeaBattle.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtTokenSettings _settings;

    public JwtTokenService(IOptions<JwtTokenSettings> config)
    {
        _settings = config.Value;
    }

    public string GenerateJwtToken(string email, int userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, userId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_settings.ExpireTime)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Result<int> GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return Result.Failure<int>(ServiceErrors.JwtTokenServiceExceptions.CannotReadToken);
        }

        var jwtToken = tokenHandler.ReadJwtToken(token);

        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            return Result.Failure<int>(ServiceErrors.JwtTokenServiceExceptions.ExpiredToken);
        }

        if (!ValidateTokenSignature(token))
        {
            return Result.Failure<int>(ServiceErrors.JwtTokenServiceExceptions.InvalidTokenSignature);
        }

        var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

        if (emailClaim == null)
        {
            return Result.Failure<int>(ServiceErrors.JwtTokenServiceExceptions.MissingEmail);
        }

        var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId);

        if (userIdClaim == null)
        {
            return Result.Failure<int>(ServiceErrors.JwtTokenServiceExceptions.MissingUserId);
        }

        return !int.TryParse(userIdClaim.Value, out int userId)
            ? Result.Failure<int>(ServiceErrors.AppUserServiceExceptions.InvalidId)
            : Result.Success(userId)!;
    }

    private bool ValidateTokenSignature(string token)
    {
        var tokenParts = token.Split('.');
        var header = tokenParts[0];
        var payload = tokenParts[1];
        var crypto = Base64UrlEncoder.DecodeBytes(tokenParts[2]);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));

        var headerAndPayload = $"{header}.{payload}";
        var bytesToSign = Encoding.UTF8.GetBytes(headerAndPayload);

        using (var hmac = new HMACSHA256(securityKey.Key))
        {
            var computedSignature = hmac.ComputeHash(bytesToSign);

            return crypto.SequenceEqual(computedSignature);
        }
    }
}