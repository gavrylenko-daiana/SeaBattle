using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class CoordinateTypeService : ICoordinateTypeService
{
    private readonly IRepository<CoordinateType> _repository;

    public CoordinateTypeService(IRepository<CoordinateType> repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<CoordinateType>> GetCoordinateTypeByTypeNameAsync(string typeName)
    {
        var coordinateType = (await _repository.GetAll()).FirstOrDefault(ct => String.Equals(ct.Type, typeName, StringComparison.CurrentCultureIgnoreCase));

        if (coordinateType is null)
        {
            return Result.Failure<CoordinateType>(ServiceErrors.CoordinateTypeServiceExceptions.CoordinateTypeNotFound);
        }

        return Result.Success(coordinateType);
    }
}