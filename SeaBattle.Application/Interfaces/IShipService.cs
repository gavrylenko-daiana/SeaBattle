using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IShipService
{
    Task<Result<BaseShip>> Insert(ShipDto shipDTO);
    Task<Result> Update(BaseShip ship);
}
