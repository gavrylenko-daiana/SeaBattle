using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Interfaces;

public interface IShipTypeService
{
    Task<Result<ShipType>> GetShipTypeByTypeNameAsync(string typeName);
}