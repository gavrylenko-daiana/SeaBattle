using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Services;

public class ValidationService : IValidationService
{
    public Result IsCorrectCoordinate(Coordinate coordinate, GameField gameField, CoordinateType coordinateType)
    {
        if (coordinate is null || gameField is null)
        {
            return Result.Failure(Error.NullValue);
        }

        var checkResult = CheckCoordinate(coordinate, gameField, coordinateType);

        if (checkResult.IsFailure)
        {
            return Result.Failure(checkResult.Error);
        }

        return Result.Success();
    }
    
    public bool IsValidPoint(Point point)
    {
        return point.X != 0 && point.Y != 0;
    }

    private Result CheckCoordinate(Coordinate coordinate, GameField gameField, CoordinateType coordinateType)
    {
        if (coordinate is null || gameField is null)
        {
            return Result.Failure(Error.NullValue);
        }
        
        if (!IsValidPoint(coordinate.Point))
        {
            return Result.Failure(DomainErrors.CoordinateExceptions.CoordinateOutsideTheQuadrant);
        }

        if (!IsCoordinateFieldBoundaries(coordinate.Point, gameField.BoundaryCoordinate))
        {
            return Result.Failure(DomainErrors.GameFieldServiceException.InvalidShipPlacement);
        }

        if (!IsEmptyCoordinate(coordinate, gameField, coordinateType))
        {
            return Result.Failure(DomainErrors.GameFieldServiceException.OccupiedCoordinate);
        }

        return Result.Success();
    }

    private bool IsCoordinateFieldBoundaries(Point coordinate, int boundaryCoordinate)
    {
        var checkX = coordinate.X >= boundaryCoordinate * (-1) && coordinate.X <= boundaryCoordinate;
        var checkY = coordinate.Y >= boundaryCoordinate * (-1) && coordinate.Y <= boundaryCoordinate;

        return checkX && checkY;
    }

    private bool IsEmptyCoordinate(Coordinate coordinate, GameField gameField, CoordinateType coordinateType)
    {
        var isEmptyCoordinate = gameField.Coordinates
            .Any(c => c.Point.X == coordinate.Point.X && c.Point.Y == coordinate.Point.Y && c.CoordinateType.Type == coordinateType.Type);

        return isEmptyCoordinate;
    }
}