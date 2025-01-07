using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Services;

public class ComputeCoordinateService : IComputeCoordinateService
{
    private readonly IValidationService _validationService;

    public ComputeCoordinateService(IValidationService validationService)
    {
        _validationService = validationService;
    }
    
    public Result<List<Coordinate>> GetAdjacentCoordinatesForShip(List<Coordinate> shipCoordinates, Direction shipDirection, GameField gameField, bool isPreparation = false)
    {
        var allAdjacentCoordinates = new List<Coordinate>();
        
        if (!isPreparation && shipDirection is Direction.Down or Direction.Left)
        {
            shipCoordinates.Reverse();
        }

        for (int i = 0; i < shipCoordinates.Count; i++)
        {
            bool isFirst = i == 0;
            bool isLast = i == shipCoordinates.Count - 1;

            var adjacentCoordinates = GetAdjacentCoordinates(shipCoordinates[i], shipDirection, gameField, isFirst, isLast);
            allAdjacentCoordinates.AddRange(adjacentCoordinates);
        }

        return Result.Success(allAdjacentCoordinates.Distinct().ToList());
    }
    
    public Coordinate? GetNextCoordinate(Coordinate coordinate, Direction direction, GameField gameField, int shipSize = 1)
    {
        var nextCoordinate = direction switch
        {
            Direction.Up => gameField.GetCoordinateOnField(new Point(coordinate.Point.X, AdjustCoordinate(coordinate.Point.Y, shipSize))),
            Direction.Down => gameField.GetCoordinateOnField(new Point(coordinate.Point.X, AdjustCoordinate(coordinate.Point.Y, -shipSize))),
            Direction.Left => gameField.GetCoordinateOnField(new Point(AdjustCoordinate(coordinate.Point.X, -shipSize), coordinate.Point.Y)),
            Direction.Right => gameField.GetCoordinateOnField(new Point(AdjustCoordinate(coordinate.Point.X, shipSize), coordinate.Point.Y)),
            _ => coordinate,
        };

        return nextCoordinate;
    }

    private List<Coordinate> GetAdjacentCoordinates(Coordinate coordinate, Direction shipDirection, GameField gameField, bool isFirst, bool isLast)
    {
        var adjacentCoordinates = new List<Coordinate>();
        var oppositeDirection = GetOppositeDirection(shipDirection);

        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            var checkDirectionFirstCoordinate = isFirst && direction != shipDirection;
            var checkDirectionLastCoordinate = isLast && direction != oppositeDirection;
            var checkDirectionOtherCoordinates = !isFirst && !isLast && direction != shipDirection && direction != oppositeDirection;
            
            if (checkDirectionFirstCoordinate || checkDirectionLastCoordinate || checkDirectionOtherCoordinates)
            {
                Coordinate? adjacent = GetNextCoordinate(coordinate, direction, gameField);

                if (adjacent is not null && _validationService.IsValidPoint(adjacent.Point))
                {
                    adjacentCoordinates.Add(adjacent);
                }
            }
        }

        if (isFirst || isLast)
        {
            var diagonalCoordinates = GetDiagonalAdjacentCoordinates(coordinate, gameField);
            adjacentCoordinates.AddRange(diagonalCoordinates);
        }

        return adjacentCoordinates;
    }

    private List<Coordinate> GetDiagonalAdjacentCoordinates(Coordinate coordinate, GameField gameField)
    {
        var coordinates = new List<Coordinate>();

        var leftDown = gameField.GetCoordinateOnField(new Point(AdjustCoordinate(coordinate.Point.X, -1), AdjustCoordinate(coordinate.Point.Y, -1)));
        var rightDown = gameField.GetCoordinateOnField(new Point(AdjustCoordinate(coordinate.Point.X, 1), AdjustCoordinate(coordinate.Point.Y, -1)));
        var leftUp = gameField.GetCoordinateOnField(new Point(AdjustCoordinate(coordinate.Point.X, -1), AdjustCoordinate(coordinate.Point.Y, 1)));
        var rightUp = gameField.GetCoordinateOnField(new Point(AdjustCoordinate(coordinate.Point.X, 1), AdjustCoordinate(coordinate.Point.Y, 1)));

        if (leftDown is not null && _validationService.IsValidPoint(leftDown.Point))
        {
            coordinates.Add(leftDown);
        }

        if (leftUp is not null && _validationService.IsValidPoint(leftUp.Point))
        {
            coordinates.Add(leftUp);
        }

        if (rightDown is not null && _validationService.IsValidPoint(rightDown.Point))
        {
            coordinates.Add(rightDown);
        }

        if (rightUp is not null && _validationService.IsValidPoint(rightUp.Point))
        {
            coordinates.Add(rightUp);
        }

        return coordinates;
    }
    
    private int AdjustCoordinate(int coordinateValue, int adjustment)
    {
        var result = coordinateValue + adjustment;
     
        if (result == 0)
        {
            result += Math.Sign(adjustment);
        }
        
        return result;
    }

    private Direction GetOppositeDirection(Direction direction)
    {
        var oppositeDirection = direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => Direction.Left
        };

        return oppositeDirection;
    }
}