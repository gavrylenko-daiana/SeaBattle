using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;
using ICoordinateService = SeaBattle.Application.Interfaces.ICoordinateService;
using IGameFieldService = SeaBattle.Application.Interfaces.IGameFieldService;

namespace SeaBattle.Application.Services;

public class ShipCoordinateService : IShipCoordinateService
{
    private readonly IRepository<ShipCoordinate> _repository;
    private readonly ICoordinateService _coordinateService;
    private readonly IComputeCoordinateService _computeCoordinateService;
    private readonly IGameFieldService _gameFieldService;
    private readonly Domain.Interfaces.IGameFieldService _fieldLogicService;
    private readonly ICoordinateTypeService _coordinateTypeService;

    public ShipCoordinateService(IRepository<ShipCoordinate> repository, ICoordinateService coordinateService,
        IComputeCoordinateService computeCoordinateService, IGameFieldService gameFieldService, Domain.Interfaces.IGameFieldService fieldLogicService, ICoordinateTypeService coordinateTypeService)
    {
        _repository = repository;
        _coordinateService = coordinateService;
        _computeCoordinateService = computeCoordinateService;
        _gameFieldService = gameFieldService;
        _fieldLogicService = fieldLogicService;
        _coordinateTypeService = coordinateTypeService;
    }
    
    public async Task<Result> AddShipToGameField(BaseShip ship, GameField gameField, Coordinate coordinate, ShipDto shipDto)
    {
        var addShipToFieldResult = await _fieldLogicService.AddShipToField(ship,
            gameField.Coordinates.Find(c =>
                c.Point.X == coordinate.Point.X &&
                c.Point.Y == coordinate.Point.Y),
            ship.Direction, gameField);

        if (addShipToFieldResult.IsFailure)
        {
            return Result.Failure<Game>(addShipToFieldResult.Error);
        }

        foreach (var shipCoordinate in ship.ShipCoordinates)
        {
            await Insert(shipCoordinate);

            if (shipCoordinate.Coordinate.CoordinateId == shipDto.CoordinateId)
            {
                shipCoordinate.Coordinate.IsFirstCoordinate = true;
            }

            await _coordinateService.Update(shipCoordinate.Coordinate);
        }

        foreach (var adjacentCoordinate in addShipToFieldResult.Value.AdjacentCoordinates)
        {
            await _coordinateService.Update(adjacentCoordinate);
        }

        return Result.Success();
    }

    public async Task<Result<ShipCoordinate>> Insert(ShipCoordinate shipCoordinate)
    {
        await _repository.Insert(shipCoordinate);

        return shipCoordinate is null
            ? Result.Failure<ShipCoordinate>(ServiceErrors.ShipCoordinateServiceExceptions.FailedCreateShipCoordinate)
            : Result.Success(shipCoordinate);
    }

    public async Task<bool> AreShipCoordinatesHits(Coordinate coordinate)
    {
        var allShipCoordinatesResult = await GetShipCoordinatesByCoordinate(coordinate);

        if (allShipCoordinatesResult.IsFailure)
        {
            return false;
        }

        var areHitsCoordinates = allShipCoordinatesResult.Value
            .Where(shc => shc.CoordinateId != coordinate.CoordinateId)
            .All(shc => shc.Coordinate.CoordinateType.Type.Equals("Hit"));

        if (areHitsCoordinates)
        {
            var destroyedTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Destroyed");
            
            if (destroyedTypeResult.IsFailure)
            {
                return false;
            }

            var destroyedType = destroyedTypeResult.Value;
            
            foreach (var shipCoordinate in allShipCoordinatesResult.Value)
            {
                if (shipCoordinate.CoordinateId != coordinate.CoordinateId)
                {
                    shipCoordinate.Coordinate.MarkCoordinateType(destroyedType);
                    await _coordinateService.Update(shipCoordinate.Coordinate);
                }
            }

            var shipCoordinateByCoordinate = coordinate.ShipCoordinates.FirstOrDefault(shc => shc.CoordinateId == coordinate.CoordinateId)!;
            var coordinates = allShipCoordinatesResult.Value.Select(shc => shc.Coordinate).ToList();
            var gameFieldResult = await _gameFieldService.GetById(coordinate.GameFieldId);
            var adjacentCoordinatesResult = _computeCoordinateService.GetAdjacentCoordinatesForShip(coordinates, shipCoordinateByCoordinate.Ship.Direction, gameFieldResult.Value);

            var missedTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Missed");
            
            if (missedTypeResult.IsFailure)
            {
                return false;
            }

            var missedType = missedTypeResult.Value;
            
            foreach (var adjacentCoordinate in adjacentCoordinatesResult.Value)
            {
                adjacentCoordinate.MarkCoordinateType(missedType);
                await _coordinateService.Update(adjacentCoordinate);
            }

            return true;
        }

        return false;
    }

    private async Task<Result<List<ShipCoordinate>>> GetShipCoordinatesByCoordinate(Coordinate coordinate)
    {
        var ship = coordinate.ShipCoordinates.FirstOrDefault(shc => shc.CoordinateId == coordinate.CoordinateId)!.Ship;

        if (ship is null)
        {
            return Result.Failure<List<ShipCoordinate>>(ServiceErrors.ShipServiceExceptions.NonExistentShip);
        }

        var allShipCoordinates = (await _repository.GetAll()).Where(shc => shc.ShipId == ship.ShipId).ToList();

        return Result.Success(allShipCoordinates);
    }
}