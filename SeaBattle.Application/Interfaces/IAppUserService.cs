using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IAppUserService
{
    Task<Result<AppUser>> GetUserById(int id);
    Task<Result<AppUser>> GetUserByEmail(string email);
    Task<Result<AppUser>> GetUserByUsername(string username);
    Task<Result<AppUser>> CreateUser(RegisterDto registerDto);
    Task<Result<List<AppUser>>> GetAllUsers(int pageNumber = 1, int pageSize = 1000);
    UserDto GetUserDto(AppUser appUser);
    Task<Result> UpdateUser(AppUser user);
}