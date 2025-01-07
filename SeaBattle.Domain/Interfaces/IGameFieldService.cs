using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Interfaces;

public interface IGameFieldService
{
    Task<Result<CoordinatesDto>> AddShipToField(BaseShip ship, Coordinate startCoordinate, 
        Direction direction, GameField gameField);
}