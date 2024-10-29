using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SeaBattle.Domain.Helpers;

namespace SeaBattle.Domain.Models;

[Table("ShipTypes")]
public class ShipType
{
    public ShipType() { }
    
    private ShipType(int id, string type)
    {
        ShipTypeId = id;
        Type = type;
    }
    
    [Key]
    [Column("ShipTypeId")]
    public int ShipTypeId { get; set; }
    
    [TypeEntity]
    [Column("Type")]
    public string Type { get; set; }

    // private static readonly Dictionary<string, ShipType> _shipTypes = new()
    // {
    //     { "war", new ShipType(1, "War") },
    //     { "support", new ShipType(2, "Support") },
    //     { "hybrid", new ShipType(3, "Hybrid") }
    // };
    //
    // public static ShipType GetShipTypeByTypeName(string typeName)
    // {
    //     if (_shipTypes.TryGetValue(typeName, out var shipType))
    //     {
    //         return shipType;
    //     }
    //
    //     throw new ArgumentException("Invalid ship type name");
    // }
}