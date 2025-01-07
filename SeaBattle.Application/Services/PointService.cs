using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class PointService : IPointService
{
    private readonly IRepository<Point> _repository;

    public PointService(IRepository<Point> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Point>> Insert(Point point)
    {
        await _repository.Insert(point);

        return point is null ? Result.Failure<Point>(ServiceErrors.PointServiceExceptions.FailedCreatePoint) : Result.Success(point);
    }
}