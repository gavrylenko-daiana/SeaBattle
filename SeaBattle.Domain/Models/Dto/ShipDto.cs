using SeaBattle.Domain.Enums;

namespace SeaBattle.Domain.Models.Dto;

public class ShipDto
{
    public int GameId { get; set; }
    public int CoordinateId { get; set; }
    public string Speed { get; set; }
    public int Size { get; set; }
    public string Direction { get; set; }
    public string ShipTypeName { get; set; }
}