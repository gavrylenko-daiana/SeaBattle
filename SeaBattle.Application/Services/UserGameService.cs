using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class UserGameService : IUserGameService
{
    private readonly IRepository<UserGames> _repository;
    private readonly IGameFieldService _gameFieldService;
    private readonly IUnitOfWork _unitOfWork;

    public UserGameService(IRepository<UserGames> repository, IGameFieldService gameFieldService, IUnitOfWork unitOfWork)
    {
        _gameFieldService = gameFieldService;
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<Result<UserGames>> Insert(Game game, AppUser currentUser)
    {
        var newGameFieldResult = await _gameFieldService.Insert();

        if (newGameFieldResult.IsFailure)
        {
            return Result.Failure<UserGames>(newGameFieldResult.Error);
        }

        var userGame = new UserGames(game, currentUser, newGameFieldResult.Value);
        await _repository.Insert(userGame);

        await _unitOfWork.SaveChanges();

        return Result.Success(userGame);
    }

    public async Task<Result> Update(UserGames userGame)
    {
        await _repository.Update(userGame);

        return Result.Success();
    }
}