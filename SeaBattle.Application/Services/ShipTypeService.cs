using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class ShipTypeService : IShipTypeService
{
    private readonly IRepository<ShipType> _repository;

    public ShipTypeService(IRepository<ShipType> repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<ShipType>> GetShipTypeByTypeNameAsync(string typeName)
    {
        var coordinateType = (await _repository.GetAll()).FirstOrDefault(ct => String.Equals(ct.Type, typeName, StringComparison.CurrentCultureIgnoreCase));

        if (coordinateType is null)
        {
            return Result.Failure<ShipType>(ServiceErrors.CoordinateTypeServiceExceptions.CoordinateTypeNotFound);
        }

        return Result.Success(coordinateType);
    }
}