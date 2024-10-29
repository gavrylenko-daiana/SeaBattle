using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Domain.Models;

[Table("CoordinateTypes")]
public class CoordinateType
{
    public CoordinateType() { }
    
    private CoordinateType(int id, string type)
    {
        CoordinateTypeId = id;
        Type = type;
    }

    [Key]
    [Column("CoordinateTypeId")]
    public int CoordinateTypeId { get; set; }
    
    [Column("Type")]
    public string Type { get; set; }
}