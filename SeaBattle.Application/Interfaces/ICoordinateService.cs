using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface ICoordinateService
{
    Task Insert(Coordinate coordinate);
    Task<Result<Coordinate>> GetById(int id);
    Task<Result> Update(Coordinate coordinate);
}