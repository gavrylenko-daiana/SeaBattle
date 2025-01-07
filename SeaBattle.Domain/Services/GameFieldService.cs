using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Services;

public class GameFieldService : IGameFieldService
{
    private readonly ICoordinateService _coordinateService;

    public GameFieldService(ICoordinateService coordinateService)
    {
        _coordinateService = coordinateService;
    }

    public async Task<Result<CoordinatesDto>> AddShipToField(BaseShip ship, Coordinate startCoordinate, 
        Direction direction, GameField gameField)
    {
        if (ship is null)
        {
            return Result.Failure<CoordinatesDto>(Error.NullValue);
        }

        var coordinatesResult = await GetShipCoordinates(ship, startCoordinate, direction, gameField);

        if (coordinatesResult.IsFailure)
        {
            return Result.Failure<CoordinatesDto>(coordinatesResult.Error);
        }

        return Result.Success(coordinatesResult.Value);
    }

    private async Task<Result<CoordinatesDto>> GetShipCoordinates(BaseShip ship, Coordinate startCoordinate, Direction direction,
            GameField gameField)
    {
        var coordinatesResult = await _coordinateService.GetFilledCoordinates(ship, startCoordinate, direction, gameField);

        if (coordinatesResult.IsFailure)
        {
            return Result.Failure<CoordinatesDto>(coordinatesResult.Error);
        }

        gameField.SetFilledCoordinates(coordinatesResult.Value.ShipCoordinates);
        gameField.SetFilledCoordinates(coordinatesResult.Value.AdjacentCoordinates);

        return Result.Success(coordinatesResult.Value);
    }
}