using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IGameFieldService
{
    Task<Result<GameField>> Insert(int fieldSize = 10);
    Task<Result> Update(GameField gameField);
    Task<Result<GameField>> GetById(int id);
}