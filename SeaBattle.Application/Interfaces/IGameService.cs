using System.Linq.Expressions;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IGameService
{
    Task<Result<List<Game>>> GetAll(Expression<Func<Game, bool>> filter = null!, Expression<Func<IQueryable<Game>, IOrderedQueryable<Game>>> orderBy = null!, int pageNumber = 1, int pageSize = 1000);
    Task<Result<Game>> GetById(int id);
    Task<Result<Game>> Insert(GameDto gameDto);
    Task<Result> Update(int id, GameDto gameDto);
    Task<Result> UpdateGameProgress(int id, GameProgress gameProgress);
    Task<Result> Delete(int id);
    Task<Result<Game>> Join(int gameId, int currentUserId);
    Task<Result<GameInvitation>> InviteUser(int gameId, int userId);
    Task<Result<Game>> AcceptInvitation(int gameId, int userId);
    Task<Result<Game>> AddShipToField(ShipDto shipDto, int userId);
    Task<Result<Game>> PlaceShipsAutomatically(int gameId, int userId);
    Task<Result<Game>> UpdateUserStatusOnReady(int gameId, int userId);
    Task<Result<Coordinate>> UpdateCoordinateType(int coordinateId);
    Task<Result> UpdatePlayerTurn(int gameId, int coordinateId, int userId);
    Task<Result<Game>> FindGame(int userId);
    Task<Result<Game>> FindOpponent(int gameId);
}