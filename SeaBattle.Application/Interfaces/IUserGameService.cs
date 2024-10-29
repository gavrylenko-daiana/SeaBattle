using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IUserGameService
{
    Task<Result<UserGames>> Insert(Game game, AppUser currentUser);
    Task<Result> Update(UserGames userGame);
}