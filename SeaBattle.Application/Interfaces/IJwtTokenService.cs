using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateJwtToken(string email, int userId);
    Result<int> GetUserIdFromToken(string token);
}