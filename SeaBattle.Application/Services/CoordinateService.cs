using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class CoordinateService : ICoordinateService
{
    private readonly IRepository<Coordinate> _repository;

    public CoordinateService(IRepository<Coordinate> repository)
    {
        _repository = repository;
    }

    public async Task Insert(Coordinate coordinate)
    {
        await _repository.Insert(coordinate);
    }

    public async Task<Result> Update(Coordinate coordinate)
    {
        await _repository.Update(coordinate);

        return Result.Success();
    }

    public async Task<Result<Coordinate>> GetById(int id)
    {
        var coordinate = await _repository.GetById(id);

        return coordinate is null ? Result.Failure<Coordinate>(ServiceErrors.CoordinateServiceExceptions.NonExistentCoordinate) : Result.Success(coordinate);
    }
}