using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Services;

public class CoordinateService : ICoordinateService
{
    private readonly IValidationService _validationService;
    private readonly IComputeCoordinateService _computeCoordinateService;
    private readonly ICoordinateTypeService _coordinateTypeService;

    public CoordinateService(IValidationService validationService, IComputeCoordinateService computeCoordinateService, ICoordinateTypeService coordinateTypeService)
    {
        _validationService = validationService;
        _computeCoordinateService = computeCoordinateService;
        _coordinateTypeService = coordinateTypeService;
    }

    public async Task<Result<CoordinatesDto>> GetFilledCoordinates(
        BaseShip ship, Coordinate startCoordinate, Direction direction, GameField gameField)
    {
        var shipCoordinates = await GenerateShipCoordinates(startCoordinate, direction, ship.Size, gameField);

        if (shipCoordinates.IsFailure)
        {
            return Result.Failure<CoordinatesDto>(shipCoordinates.Error);
        }

        var adjacentCoordinates =
            _computeCoordinateService.GetAdjacentCoordinatesForShip(shipCoordinates.Value, direction, gameField, true);

        if (adjacentCoordinates.IsFailure)
        {
            return Result.Failure<CoordinatesDto>(adjacentCoordinates.Error);
        }

        ship.AddCoordinatesToShip(ship, shipCoordinates.Value);

        var markResult = await MarkCoordinatesAsFilledAsync(shipCoordinates.Value, adjacentCoordinates.Value);
        
        if (markResult.IsFailure)
        {
            return Result.Failure<CoordinatesDto>(markResult.Error);
        }

        var coordinates = new CoordinatesDto()
        {
            ShipCoordinates = shipCoordinates.Value,
            AdjacentCoordinates = adjacentCoordinates.Value
        };

        return Result.Success(coordinates);
    }
    
    private async Task<Result<List<Coordinate>>> GenerateShipCoordinates(Coordinate startCoordinate, Direction direction, int shipSize,
        GameField gameField)
    {
        var coordinates = new List<Coordinate>();
        var currentCoordinate = startCoordinate;
        var emptyTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Empty");
        
        if (emptyTypeResult.IsFailure)
        {
            return Result.Failure<List<Coordinate>>(emptyTypeResult.Error);
        }

        for (int i = 1; i <= shipSize; i++)
        {
            var resultCheck = _validationService.IsCorrectCoordinate(currentCoordinate, gameField, emptyTypeResult.Value);
            
            if (resultCheck.IsFailure)
            {
                return Result.Failure<List<Coordinate>>(resultCheck.Error);
            }

            coordinates.Add(currentCoordinate);
            currentCoordinate = _computeCoordinateService.GetNextCoordinate(currentCoordinate, direction, gameField);
        }

        return Result.Success(coordinates);
    }
    
    private async Task<Result<CoordinateType>> GetFilledCoordinateType()
    {
        var filledTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Filled");
    
        if (filledTypeResult.IsFailure)
        {
            return Result.Failure<CoordinateType>(filledTypeResult.Error);
        }

        return Result.Success(filledTypeResult.Value);
    }

    private async Task<Result> MarkCoordinatesAsFilledAsync(List<Coordinate> shipCoordinates, List<Coordinate> adjacentCoordinates)
    {
        var filledTypeResult = await GetFilledCoordinateType();
        
        if (filledTypeResult.IsFailure)
        {
            return Result.Failure(filledTypeResult.Error);
        }

        shipCoordinates.ForEach(coordinate => coordinate.MarkCoordinateType(filledTypeResult.Value));
        adjacentCoordinates.ForEach(coordinate => coordinate.MarkCoordinateType(filledTypeResult.Value));

        return Result.Success();
    }
}