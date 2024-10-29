using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class ShipService : IShipService
{
    private readonly IRepository<BaseShip> _repository;
    private readonly IShipTypeService _shipTypeService;

    public ShipService(IRepository<BaseShip> repository, IShipTypeService shipTypeService)
    {
        _repository = repository;
        _shipTypeService = shipTypeService;
    }

    public async Task<Result<BaseShip>> Insert(ShipDto shipDTO)
    {
        if (!Enum.TryParse(shipDTO.Direction, ignoreCase: true, out Direction parsedDirection))
        {
            return Result.Failure<BaseShip>(ServiceErrors.ShipServiceExceptions.CannotConvertDirection);
        }
        
        var shipTypeResult = await _shipTypeService.GetShipTypeByTypeNameAsync(shipDTO.ShipTypeName);

        if (shipTypeResult.IsFailure)
        {
            return Result.Failure<BaseShip>(shipTypeResult.Error);
        }

        var createShipResult = BaseShip.CreateShip(
            int.Parse(shipDTO.Speed),
            shipDTO.Size,
            parsedDirection, 
            shipTypeResult.Value);

        if (createShipResult.IsFailure)
        {
            return Result.Failure<BaseShip>(createShipResult.Error);
        }

        await _repository.Insert(createShipResult.Value);

        return createShipResult.Value is null ? Result.Failure<BaseShip>(ServiceErrors.ShipServiceExceptions.NonExistentShip) : Result.Success(createShipResult.Value);
    }

    public async Task<Result> Update(BaseShip ship)
    {
        await _repository.Update(ship);

        return Result.Success();
    }
}