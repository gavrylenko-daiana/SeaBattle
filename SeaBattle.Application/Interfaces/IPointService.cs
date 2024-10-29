using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IPointService
{
    Task<Result<Point>> Insert(Point point);
}