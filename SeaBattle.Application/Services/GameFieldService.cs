using Microsoft.EntityFrameworkCore;
using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;
using ICoordinateService = SeaBattle.Application.Interfaces.ICoordinateService;
using IGameFieldService = SeaBattle.Application.Interfaces.IGameFieldService;

namespace SeaBattle.Application.Services;

public class GameFieldService : IGameFieldService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<GameField> _repository;
    private readonly IPointService _pointService;
    private readonly ICoordinateService _coordinateService;
    private readonly ICoordinateTypeService _coordinateTypeService;

    public GameFieldService(IRepository<GameField> repository, IPointService pointService,
        ICoordinateService coordinateService, IUnitOfWork unitOfWork, ICoordinateTypeService coordinateTypeService)
    {
        _pointService = pointService;
        _coordinateService = coordinateService;
        _unitOfWork = unitOfWork;
        _coordinateTypeService = coordinateTypeService;
        _repository = repository;
    }

    public async Task<Result<GameField>> Insert(int fieldSize)
    {
        var gameFieldResult = GameField.CreateGameField(fieldSize);

        if (gameFieldResult.IsFailure)
        {
            return Result.Failure<GameField>(gameFieldResult.Error);
        }
        
        var emptyTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Empty");
    
        if (emptyTypeResult.IsFailure)
        {
            throw new Exception(emptyTypeResult.Error.Message);
        }

        gameFieldResult.Value.SetCoordinatesToGameField(emptyTypeResult.Value);
        
        if (gameFieldResult.IsFailure)
        {
            return Result.Failure<GameField>(gameFieldResult.Error);
        }
        
        var gameField = gameFieldResult.Value;
        await _repository.Insert(gameField);
        
        var saveResult = await _unitOfWork.SaveChanges();

        if (!saveResult)
        {
            Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
        }
        
        if (gameField is null)
        {
            return Result.Failure<GameField>(ServiceErrors.GameFieldServiceExceptions.FailedCreateGameField);
        }

        // foreach (var coordinate in gameField.Coordinates)
        // {
        //     var newPointResult = await _pointService.Insert(coordinate.Point);
        //
        //     if (newPointResult.IsFailure)
        //     {
        //         return Result.Failure<GameField>(newPointResult.Error);
        //     }
        //
        //     coordinate.PointId = newPointResult.Value.PointId;
        //     coordinate.GameFieldId = gameField.GameFieldId;
        //
        //     await _coordinateService.Insert(coordinate);
        // }

        return Result.Success(gameField);
    }

    public async Task<Result> Update(GameField gameField)
    {
        await _repository.Update(gameField);

        return Result.Success();
    }

    public async Task<Result<GameField>> GetById(int id)
    {
        var gameField = await _repository.GetById(
            id,
            query => query.Include(gf => gf.Coordinates).ThenInclude(c => c.Point)
            );

        return gameField is null ? Result.Failure<GameField>(ServiceErrors.GameFieldServiceExceptions.NonExistentGameField) : Result.Success(gameField);
    }
}