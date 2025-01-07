using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Domain.Models;

// [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
// [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
[Table("Points")]
public record Point
{
    public Point() { }
    
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
     
    [Key]
    [Column("PointId")]
    public int PointId { get; set; }

    [Column("X")]
    public int X { get; private set; }
    
    [Column("Y")]
    public int Y { get; private set; }
}