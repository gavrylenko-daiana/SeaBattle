using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<UserDto>> Authenticate(LoginDto loginDto);
}