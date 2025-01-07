using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.Application.Services;

public class GameInvitationService : IGameInvitationService
{
    private readonly IRepository<GameInvitation> _repository;

    public GameInvitationService(IRepository<GameInvitation> repository)
    {
        _repository = repository;
    }

    public async Task<Result<GameInvitation>> CreateNewInvitation(Game game, AppUser appUser)
    {
        var checkInvitation = await IsInvitationExist(game, appUser);

        if (checkInvitation.IsFailure)
        {
            return Result.Failure<GameInvitation>(checkInvitation.Error);
        }

        var gameInvitation = new GameInvitation(game, appUser);
        await _repository.Insert(gameInvitation);

        return Result.Success(gameInvitation);
    }

    public async Task<Result> IsInvitationExist(Game game, AppUser appUser)
    {
        var allInvitations = (await _repository.GetAll()).ToList();

        if (allInvitations.Any())
        { 
            var existedInvitation = allInvitations.FirstOrDefault(inv => inv.GameId == game.GameId && inv.AppUserId == appUser.AppUserId);

            if (existedInvitation is null)
            {
                return Result.Success();
            }
        }

        return Result.Failure(ServiceErrors.GameInvitationServiceExceptions.InvitationAlreadyExists);
    }
    
    public async Task<Result<List<GameInvitation>>> GetAll(int pageNumber = 1, int pageSize = 1000)
    {
        var invitations = (await _repository.GetAll()).ToList();

        return Result.Success(invitations);
    }
}