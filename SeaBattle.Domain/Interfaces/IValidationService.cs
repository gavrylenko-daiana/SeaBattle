using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Interfaces;

public interface IValidationService
{
    Result IsCorrectCoordinate(Coordinate startCoordinate, GameField gameField, CoordinateType coordinateType);

    bool IsValidPoint(Point point);
}