using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Interfaces;

public interface IComputeCoordinateService
{
    Result<List<Coordinate>> GetAdjacentCoordinatesForShip(List<Coordinate> shipCoordinates, Direction shipDirection, GameField gameField, bool isPreparation = false);

    Coordinate GetNextCoordinate(Coordinate coordinate, Direction direction, GameField gameField, int shipSize = 1);
}