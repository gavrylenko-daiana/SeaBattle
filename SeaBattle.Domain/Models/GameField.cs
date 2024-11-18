using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Models;

[Table("GameFields")]
public class GameField
{
    public GameField() { }
    
    private GameField(int fieldSize)
    {
        FieldSize = fieldSize;
        BoundaryCoordinate = FieldSize / 2;
    }
    
    public List<Coordinate> Coordinates { get; private set; } = new();

    [Key]
    [Column("GameFieldId")]
    public int GameFieldId { get; set; }
    
    [Column("BoundaryCoordinate")]
    public int BoundaryCoordinate { get; private set; }

    [Column("FieldSize")]
    public int FieldSize { get; set; }

    public Result<BaseShip> this[Quadrant quadrant, Point coordinate]
    {
        get
        {
            var coordinateObj = Coordinates.FirstOrDefault(c => c.Quadrant == quadrant && c.Point.X == coordinate.X && c.Point.Y == coordinate.Y);
            
            if (coordinateObj is null)
            {
                return Result.Failure<BaseShip>(Error.NullValue);
            }
            
            var shipCoordinate = coordinateObj.ShipCoordinates.FirstOrDefault();
            
            if (shipCoordinate is null)
            {
                return Result.Failure<BaseShip>(Error.NullValue);
            }
    
            return shipCoordinate.Ship is null
                ? Result.Failure<BaseShip>(DomainErrors.BaseShipExceptions.NotExistShip)
                : Result.Success<BaseShip>(shipCoordinate.Ship);
        }
    }

    public static Result<GameField> CreateGameField(int fieldSize)
    {
        var sizeResult = IsCorrectFieldSize(fieldSize);

        if (sizeResult.IsFailure)
        {
            return Result.Failure<GameField>(sizeResult.Error);
        }

        return Result.Success(new GameField(sizeResult.Value));
    }
    
    public void SetCoordinatesToGameField(CoordinateType coordinateType)
    {
        for (int x = -BoundaryCoordinate; x <= BoundaryCoordinate; x++)
        {
            for (int y = -BoundaryCoordinate; y <= BoundaryCoordinate; y++)
            {
                if (x == 0 || y == 0)
                {
                    continue;
                }

                var newCoordinate = Coordinate.CreateCoordinate(new Point(x, y), this, coordinateType);

                Coordinates.Add(newCoordinate.Value);
            }
        }
    }

    public Coordinate? GetCoordinateOnField(Point point)
    {
        Coordinate? getCoordinate = Coordinates.FirstOrDefault(c => c.Point.X == point.X && c.Point.Y == point.Y);

        // if (getCoordinate is null)
        // {
        //     throw new Exception("Coordinate not found on the game field.");
        // }

        return getCoordinate;
    }

    public void SetFilledCoordinates(List<Coordinate> coordinates)
    {
        foreach (var coordinate in coordinates)
        {
            var index = Coordinates.FindIndex(c => c.Point.X == coordinate.Point.X && c.Point.Y == coordinate.Point.Y);
            Coordinates[index] = coordinate;
            Coordinates[index].CoordinateId = coordinate.CoordinateId;
        }
    }
    
    public override string ToString()
    {
        var shipStates = GetSortedShips().Select(ship => ship.ToString());

        return $"\n{string.Join("\n", shipStates)}";
    }

    private static Result<int> IsCorrectFieldSize(int fieldSize)
    {
        const int minFieldSize = 10;
        const int maxFieldSize = 100;
        var isEvenFieldSize = fieldSize % 2 != 0;

        if (fieldSize < minFieldSize)
        {
            return Result.Failure<int>(DomainErrors.GameFieldExceptions.LessThan10FieldSize);
        }

        if (fieldSize > maxFieldSize)
        {
            return Result.Failure<int>(DomainErrors.GameFieldExceptions.MoreThan100FieldSize);
        }

        if (isEvenFieldSize)
        {
            return Result.Failure<int>(DomainErrors.GameFieldExceptions.NotEvenFieldSize);
        }

        return Result.Success<int>(fieldSize);
    }

    private List<BaseShip> GetSortedShips()
    {
        var ships = Coordinates
            .SelectMany(coord => coord.ShipCoordinates)
            .Select(shipCoord => shipCoord.Ship)
            .Distinct()
            .ToList();
        
        return ships.OrderBy(ship => ship.GetDistanceFromCenter() + ship.Size / 2.0).ToList();
    }
}