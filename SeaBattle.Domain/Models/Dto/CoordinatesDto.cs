namespace SeaBattle.Domain.Models.Dto;

public class CoordinatesDto
{
    public List<Coordinate> AdjacentCoordinates { get; set; } = new List<Coordinate>();
    public List<Coordinate> ShipCoordinates { get; set; } = new List<Coordinate>();
}