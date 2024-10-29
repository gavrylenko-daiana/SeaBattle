using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Helpers;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Models;

[Table("Ships")]
public abstract class BaseShip
{
    protected BaseShip(int speed, int size, Direction direction, ShipType shipType)
    {
        Speed = speed;
        Size = size;
        Direction = direction;
        ShipType = shipType;
        ShipTypeId = shipType.ShipTypeId;
    }

    protected BaseShip() { }

    [Key]
    [Column("ShipId")]
    public int ShipId { get; set; }

    [Column("Range")]
    public int Range
    {
        get => Size;
        private set => value = 0;
    }
    
    [Column("Direction")]
    public Direction Direction { get; private set; }
    
    [Column("Size")]
    public int Size { get; private set; }
    
    [TypeEntity(Name = "ShipTypes")]
    [ForeignKey("ShipTypeId")]
    public ShipType ShipType { get; private set; }

    [Column("ShipTypeId")]
    public int ShipTypeId { get; private set; }
    
    [Column("Speed")]
    public int Speed { get; private set; }
    
    public List<ShipCoordinate> ShipCoordinates { get; private set; } = new();

    public static Result<BaseShip> CreateShip(int speed, int size, Direction direction, ShipType shipType)
    {
        var shipSizeResult = IsCorrectShipSize(size);
        
        if (shipSizeResult.IsFailure)
        {
            return Result.Failure<BaseShip>(shipSizeResult.Error);
        }

        var shipSpeedResult = IsCorrectShipSpeed(speed);
        
        if (shipSpeedResult.IsFailure)
        {
            return Result.Failure<BaseShip>(shipSpeedResult.Error);
        }

        return shipType.Type switch
        {
            "War" => Result.Success<BaseShip>(new WarShip(shipSpeedResult.Value, shipSizeResult.Value, direction, shipType)),
            "Hybrid" => Result.Success<BaseShip>(new HybridShip(shipSpeedResult.Value, shipSizeResult.Value, direction, shipType)),
            "Support" => Result.Success<BaseShip>(new SupportShip(shipSpeedResult.Value, shipSizeResult.Value, direction, shipType)),
            _ => Result.Failure<BaseShip>(DomainErrors.BaseShipExceptions.NotExistTypeOfShip)
        };
    }

    public double GetDistanceFromCenter()
    {
        return ShipCoordinates.Select(coord =>
                Math.Sqrt(Math.Pow(coord.Coordinate.Point.X, 2) + Math.Pow(coord.Coordinate.Point.Y, 2)))
            .DefaultIfEmpty(0)
            .Average();
    }

    public void AddCoordinatesToShip(BaseShip ship, List<Coordinate> coordinates)
    {
        foreach (var coordinate in coordinates)
        {
            var shipCoordinate = ShipCoordinate.CreateShipCoordinate(ship, coordinate);
            ShipCoordinates.Add(shipCoordinate);
            coordinate.ShipCoordinates.Add(shipCoordinate);
        }
    }

    public static bool operator ==(BaseShip firstShip, BaseShip secondShip)
    {
        if (firstShip is null)
        {
            throw new ArgumentNullException(nameof(firstShip));
        }
        
        if (secondShip is null)
        {
            throw new ArgumentNullException(nameof(secondShip));
        }

        return firstShip.Equals(secondShip);
    }

    public static bool operator !=(BaseShip firstShip, BaseShip secondShip)
    {
        if (firstShip is null)
        {
            throw new ArgumentNullException(nameof(firstShip));
        }
        
        if (secondShip is null)
        {
            throw new ArgumentNullException(nameof(secondShip));
        }
        
        return !(firstShip == secondShip);
    }

    public override string ToString()
    {
        return $"ShipType: {ShipType.Type}, Speed: {Speed}, Size: {Size}, Range: {Range} Coordinates: [{string.Join(", ", ShipCoordinates.Select(c => $"{c.Coordinate.Quadrant}: ({c.Coordinate.Point.X}; {c.Coordinate.Point.Y})"))}]";
    }

    private static Result<int> IsCorrectShipSize(int size)
    {
        const int minShipSize = 0;
        const int maxShipSize = 4;

        if (size <= minShipSize)
        {
            return Result.Failure<int>(DomainErrors.BaseShipExceptions.LessThan0ShipSize);
        }

        if (size > maxShipSize)
        {
            return Result.Failure<int>(DomainErrors.BaseShipExceptions.MoreThan4ShipSize);
        }

        return Result.Success<int>(size);
    }
    
    private static Result<int> IsCorrectShipSpeed(int speed)
    {
        const int minShipSpeed = 0;
        
        if (speed <= minShipSpeed)
        {
            return Result.Failure<int>(DomainErrors.BaseShipExceptions.LessThan0ShipSpeed);
        }

        return Result.Success<int>(speed);
    }
}