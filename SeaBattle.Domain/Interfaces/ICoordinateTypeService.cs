using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Domain.Interfaces;

public interface ICoordinateTypeService
{
    Task<Result<CoordinateType>> GetCoordinateTypeByTypeNameAsync(string typeName);
}