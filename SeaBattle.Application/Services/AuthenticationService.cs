using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAppUserService _userService;
    private readonly IPasswordHasherService _passwordHasherService;

    public AuthenticationService(IAppUserService userService, IPasswordHasherService passwordHasherService)
    {
        _userService = userService;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<Result<UserDto>> Authenticate(LoginDto loginDto)
    {
        var userResult = await _userService.GetUserByEmail(loginDto.Email);
        
        if (userResult.IsFailure)
        {
            return Result.Failure<UserDto>(ServiceErrors.AppUserServiceExceptions.InvalidEmail);
        }
        
        var loginPasswordHash = _passwordHasherService.GetPasswordHash(loginDto.Password);

        if (!userResult.Value.PasswordHash.Equals(loginPasswordHash))
        {
            return Result.Failure<UserDto>(ServiceErrors.AppUserServiceExceptions.InvalidPassword);
        }

        var user = _userService.GetUserDto(userResult.Value);
        
        return Result.Success(user);
    }
}