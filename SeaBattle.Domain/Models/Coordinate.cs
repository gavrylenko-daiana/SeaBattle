using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Helpers;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Models;

[Table("Coordinates")]
public record Coordinate
{
    public Coordinate() { }
    
    private Coordinate(Point point, Quadrant quadrant, CoordinateType coordinateType, GameField gameField)
    {
        Point = point;
        Quadrant = quadrant;
        CoordinateType = coordinateType;
        CoordinateTypeId = coordinateType.CoordinateTypeId;
        GameField = gameField;
        GameFieldId = gameField.GameFieldId;
    }
    
    [Key]
    [Column("CoordinateId")]
    public int CoordinateId { get; set; }
    
    [ForeignKey("PointId")]
    public Point Point { get; private set; }
    
    [Column("PointId")]
    public int PointId { get; set; }
    
    [Column("Quadrant")]
    public Quadrant Quadrant { get; private set; }
    
    [TypeEntity]
    [ForeignKey("CoordinateTypeId")]
    public CoordinateType CoordinateType { get; set; }

    [Column("CoordinateTypeId")]
    public int CoordinateTypeId { get; private set; }
    
    [ForeignKey("GameFieldId")]
    public GameField GameField { get; private set; }
    
    [Column("GameFieldId")]
    public int GameFieldId { get; set; }
    
    [Column("IsFirstCoordinate")]
    public bool IsFirstCoordinate { get; set; }

    public List<ShipCoordinate> ShipCoordinates { get; private set; } = new();
    
    public static Result<Coordinate> CreateCoordinate(Point point, GameField gameField, CoordinateType coordinateType)
    {
        var quadrantResult = GetQuadrant(point);

        if (quadrantResult.IsFailure)
        {
            return Result.Failure<Coordinate>(quadrantResult.Error);
        }

        return Result.Success(new Coordinate(point, quadrantResult.Value, coordinateType, gameField));
    }
    
    public void MarkCoordinateType(CoordinateType type)
    {
        CoordinateType = type;
        CoordinateTypeId = type.CoordinateTypeId;
    }
    
    private static Result<Quadrant> GetQuadrant(Point point)
    {
        return point switch
        {
            { X: > 0, Y: > 0 } => Result.Success(Quadrant.First),
            { X: < 0, Y: > 0 } => Result.Success(Quadrant.Second),
            { X: < 0, Y: < 0 } => Result.Success(Quadrant.Third),
            _ => Result.Success(Quadrant.Fourth)
        };
    }
}