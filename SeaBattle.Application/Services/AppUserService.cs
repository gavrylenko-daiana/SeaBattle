using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class AppUserService : IAppUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<AppUser> _repository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public AppUserService(IUnitOfWork unitOfWork, IRepository<AppUser> repository,
        IPasswordHasherService passwordHasherService, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AppUser>> CreateUser(RegisterDto registerDto)
    {
        if (!(await GetUserByEmail(registerDto.Email)).IsFailure)
        {
            return Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.UserEmailIsAlreadyExists);
        }

        if (!(await GetUserByUsername(registerDto.UserName)).IsFailure)
        {
            return Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.UserNameIsAlreadyExists);
        }

        var hashedPassword = _passwordHasherService.GetPasswordHash(registerDto.Password);

        var user = new AppUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            PasswordHash = hashedPassword,
            Rating = 1000 // Set initial rating
        };

        await _repository.Insert(user);

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success(user) : Result.Failure<AppUser>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<AppUser>> GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NullOrWhiteSpaceString);
        }

        var users = await _repository.GetAll();
        var user = users.FirstOrDefault(u => u.Email.Equals(email));

        return user is null ? Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NonExistentUser) : Result.Success(user);
    }

    public async Task<Result<AppUser>> GetUserByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NullOrWhiteSpaceString);
        }

        var user = (await _repository.GetAll()).FirstOrDefault(u => u.UserName.Equals(username));

        return user is null ? Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NonExistentUser) : Result.Success(user);
    }

    public async Task<Result<AppUser>> GetUserById(int id)
    {
        if (id < 0)
        {
            return Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.InvalidId);
        }

        var user = await _repository.GetById(id);

        return user is null ? Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NonExistentUser) : Result.Success(user);
    }

    public async Task<Result<List<AppUser>>> GetAllUsers(int pageNumber = 1, int pageSize = 1000)
    {
        var users = (await _repository.GetAll()).ToList();

        return !users.Any() ? Result.Failure<List<AppUser>>(ServiceErrors.AppUserServiceExceptions.NonExistentUsers) : Result.Success(users);
    }

    public UserDto GetUserDto(AppUser appUser)
    {
        var user = new UserDto
        {
            AppUserId = appUser.AppUserId,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            UserName = appUser.UserName,
            Email = appUser.Email,
            Token = _jwtTokenService.GenerateJwtToken(appUser.Email, appUser.AppUserId),
            Rating = appUser.Rating,
            UserGames = appUser.UserGames
        };

        return user;
    }

    public async Task<Result> UpdateUser(AppUser user)
    {
        try
        {
            await _repository.Update(user);
            var saveResult = await _unitOfWork.SaveChanges();
            return saveResult 
                ? Result.Success() 
                : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
        }
        catch (Exception ex)
        {
            return Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NonExistentUser);
        }
    }
}