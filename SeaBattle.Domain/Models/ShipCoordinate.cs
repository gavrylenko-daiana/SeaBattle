using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Domain.Models;

[Table("ShipCoordinates")]
public class ShipCoordinate
{
    public ShipCoordinate() { }
        
    private ShipCoordinate(BaseShip ship, Coordinate coordinate)
    {
        Coordinate = coordinate;
        CoordinateId = coordinate.CoordinateId;
        Ship = ship;
        ShipId = ship.ShipId;
    }

    [Key]
    [Column("ShipCoordinateId")]
    public int ShipCoordinateId { get; private set; }
    
    [ForeignKey("CoordinateId")]
    public Coordinate Coordinate { get; private set; }
    
    [Column("CoordinateId")]
    public int CoordinateId { get; private set; }
    
    [ForeignKey("ShipId")]
    public BaseShip Ship { get; private set; }
    
    [Column("ShipId")]
    public int ShipId { get; private set; }

    public static ShipCoordinate CreateShipCoordinate(BaseShip ship, Coordinate coordinate)
    {
        if (ship is null || coordinate is null)
        {
            throw new ArgumentNullException(nameof(ship));
        }
        
        return new ShipCoordinate(ship, coordinate);
    }
}