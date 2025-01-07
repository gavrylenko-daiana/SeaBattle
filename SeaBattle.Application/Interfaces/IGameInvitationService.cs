using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Interfaces;

public interface IGameInvitationService
{
    Task<Result<GameInvitation>> CreateNewInvitation(Game game, AppUser appUser);
    Task<Result> IsInvitationExist(Game game, AppUser appUser);
    Task<Result<List<GameInvitation>>> GetAll(int pageNumber = 1, int pageSize = 1000);
}