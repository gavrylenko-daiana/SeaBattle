using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IShipCoordinateService
{
    Task<Result<ShipCoordinate>> Insert(ShipCoordinate shipCoordinate);
    Task<bool> AreShipCoordinatesHits(Coordinate coordinate);
    Task<Result> AddShipToGameField(BaseShip ship, GameField gameField, Coordinate coordinate, ShipDto shipDto);
}