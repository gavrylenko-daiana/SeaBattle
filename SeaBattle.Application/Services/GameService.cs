using System.Linq.Expressions;
using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;
using ICoordinateService = SeaBattle.Application.Interfaces.ICoordinateService;
using IGameFieldService = SeaBattle.Application.Interfaces.IGameFieldService;

namespace SeaBattle.Application.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Game> _repository;
    private readonly IUserGameService _userGameService;
    private readonly IAppUserService _userService;
    private readonly IGameInvitationService _invitationService;
    private readonly IShipService _shipService;
    private readonly ICoordinateService _coordinateService;
    private readonly IShipCoordinateService _shipCoordinateService;
    private readonly ICoordinateTypeService _coordinateTypeService;
    private readonly IGameFieldService _gameFieldService;

    public GameService(IRepository<Game> repository, IUnitOfWork unitOfWork, IUserGameService userGameService,
        IAppUserService userService, IGameInvitationService invitationService, IShipService shipService, ICoordinateService coordinateService,
        IShipCoordinateService shipCoordinateService, IGameFieldService gameFieldService)
    {
        _unitOfWork = unitOfWork;
        _userGameService = userGameService;
        _userService = userService;
        _invitationService = invitationService;
        _shipService = shipService;
        _coordinateService = coordinateService;
        _shipCoordinateService = shipCoordinateService;
        _gameFieldService = gameFieldService;
        _repository = repository;
    }

    public async Task<Result<List<Game>>> GetAll(Expression<Func<Game, bool>> filter = null!, Expression<Func<IQueryable<Game>, IOrderedQueryable<Game>>> orderBy = null!, int pageNumber = 1, int pageSize = 1000)
    {
        var games = await _repository.GetAll(filter, orderBy);

        var paginatedGames = games.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return paginatedGames.Any() 
            ? Result.Success(paginatedGames) 
            : Result.Failure<List<Game>>(ServiceErrors.GameServiceExceptions.NonExistentGames);
    }
    
    public async Task<Result<Game>> GetById(int id)
    {
        var game = await _repository.GetById(id);

        return game is null ? Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NonExistentGame) : Result.Success(game);
    }

    public async Task<Result<Game>> Insert(GameDto gameDto)
    {
        var currentUserResult = await _userService.GetUserById(gameDto.CreatorId!.Value);

        if (currentUserResult.IsFailure)
        {
            return Result.Failure<Game>(currentUserResult.Error);
        }

        var game = new Game(gameDto.Name, gameDto.CreatorId!.Value);
        
        await _repository.Insert(game);

        var userGameResult = await _userGameService.Insert(game, currentUserResult.Value);

        if (userGameResult.IsFailure)
        {
            return Result.Failure<Game>(userGameResult.Error);
        }

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success(game) : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result> Update(int id, GameDto gameDto)
    {
        var createGameResult = new Game(gameDto.Name, gameDto.CreatorId!.Value)
        {
            GameId = id
        };

        await _repository.Update(createGameResult);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success() : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result> Delete(int id)
    {
        var game = await _repository.GetById(id);

        if (game is null)
        {
            return Result.Failure(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }
        
        var invitations = await _invitationService.GetAll();

        if (!invitations.IsFailure)
        {
            var invitationsCurrentGame = invitations.Value.Where(i => i.GameId == id).ToList();
            game.GameInvitations = invitationsCurrentGame;
        }
        
        await _repository.Delete(game);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success() : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<Game>> Join(int gameId, int currentUserId)
    {
        var game = await _repository.GetById(gameId);

        if (game is null)
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }

        if (game.GameUsers.Any(gameUser => gameUser.AppUserId == currentUserId))
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.UserHasAlreadyJoinedThisGame);
        }

        if (game.GameUsers.Count >= 2)
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.GameHasAlreadyStarted);
        }

        var currentUserResult = await _userService.GetUserById(currentUserId);

        if (currentUserResult.IsFailure)
        {
            return Result.Failure<Game>(currentUserResult.Error);
        }

        var userGameResult = await _userGameService.Insert(game, currentUserResult.Value);

        if (userGameResult.IsFailure)
        {
            return Result.Failure<Game>(userGameResult.Error);
        }

        game.Progress = GameProgress.GameOn;

        await _repository.Update(game);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success(game) : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<GameInvitation>> InviteUser(int gameId, int userId)
    {
        var game = await _repository.GetById(gameId);

        if (game is null)
        {
            return Result.Failure<GameInvitation>(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }

        var userResult = await _userService.GetUserById(userId);

        if (userResult.IsFailure)
        {
            return Result.Failure<GameInvitation>(userResult.Error);
        }

        var invitationResult = await _invitationService.CreateNewInvitation(game, userResult.Value);

        if (invitationResult.IsFailure)
        {
            return Result.Failure<GameInvitation>(invitationResult.Error);
        }

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success(invitationResult.Value) : Result.Failure<GameInvitation>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<Game>> AcceptInvitation(int gameId, int userId)
    {
        var game = await _repository.GetById(gameId);

        if (game is null)
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }

        var userResult = await _userService.GetUserById(userId);

        if (userResult.IsFailure)
        {
            return Result.Failure<Game>(userResult.Error);
        }

        var checkInvitation = await _invitationService.IsInvitationExist(game, userResult.Value);

        if (checkInvitation.IsFailure)
        {
            return Result.Failure<Game>(checkInvitation.Error);
        }

        var joinGameResult = await Join(gameId, userId);

        return joinGameResult.IsFailure
            ? Result.Failure<Game>(joinGameResult.Error)
            : Result.Success(joinGameResult.Value);
    }

    public async Task<Result<Game>> AddShipToField(ShipDto shipDto, int userId)
    {
        var getGameResult = await GetById(shipDto.GameId);

        if (getGameResult.IsFailure)
        {
            return Result.Failure<Game>(getGameResult.Error);
        }

        var getShipResult = await _shipService.Insert(shipDto);

        if (getShipResult.IsFailure)
        {
            return Result.Failure<Game>(getShipResult.Error);
        }

        var getCoordinateResult = await _coordinateService.GetById(shipDto.CoordinateId);

        if (getCoordinateResult.IsFailure)
        {
            return Result.Failure<Game>(getCoordinateResult.Error);
        }

        var userGame = getGameResult.Value.GameUsers.FirstOrDefault(gu => gu.AppUserId == userId);

        if (userGame?.GameField is null)
        {
            return Result.Failure<Game>(ServiceErrors.UserGameFieldServiceExceptions.NonExistentUserGame);
        }
        
        var ship = getShipResult.Value;

        if (ship is null)
        {
            return Result.Failure<Game>(ServiceErrors.ShipServiceExceptions.NonExistentShip);
        }

        var result = await _shipCoordinateService.AddShipToGameField(ship, userGame.GameField, getCoordinateResult.Value, shipDto);

        if (result.IsFailure)
        {
            return Result.Failure<Game>(result.Error);
        }

        await _shipService.Update(ship);
        await _gameFieldService.Update(userGame.GameField);

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success(getGameResult.Value) : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<Game>> UpdateUserStatusOnReady(int gameId, int userId)
    {
        var game = await _repository.GetById(gameId);

        if (game is null)
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }

        var userGame = game.GameUsers.FirstOrDefault(ug => ug.AppUserId == userId);

        if (userGame is null)
        {
            return Result.Failure<Game>(ServiceErrors.UserGameFieldServiceExceptions.NonExistentUserGame);
        }
        
        userGame.IsReady = true;

        var userGameWithoutCurrentUser = game.GameUsers.FirstOrDefault(ug => ug.AppUserId != userId);

        if (userGameWithoutCurrentUser is null)
        {
            return Result.Failure<Game>(ServiceErrors.UserGameFieldServiceExceptions.NonExistentUserGame);
        }

        if (userGameWithoutCurrentUser.IsReady)
        {
            userGame.IsPlayerTurn = true;
        }

        await _userGameService.Update(userGame);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success(game) : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<Coordinate>> UpdateCoordinateType(int coordinateId)
    {
        var coordinateResult = await _coordinateService.GetById(coordinateId);

        if (coordinateResult.IsFailure)
        {
            return Result.Failure<Coordinate>(coordinateResult.Error);
        }

        var coordinate = coordinateResult.Value;
        var isShipCoordinate = coordinate.ShipCoordinates.Any(shc => shc.CoordinateId == coordinate.CoordinateId);

        if (isShipCoordinate && coordinate.CoordinateType.Type.Equals("Filled"))
        {
            if (await _shipCoordinateService.AreShipCoordinatesHits(coordinate))
            {
                var destroyedTypeResult =
                    await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Destroyed");
                if (destroyedTypeResult.IsFailure)
                {
                    return Result.Failure<Coordinate>(ServiceErrors.CoordinateTypeServiceExceptions.CoordinateTypeNotFound);
                }

                coordinate.MarkCoordinateType(destroyedTypeResult.Value);
            }
            else
            {
                var hitTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Hit");
                if (hitTypeResult.IsFailure)
                {
                    return Result.Failure<Coordinate>(ServiceErrors.CoordinateTypeServiceExceptions.CoordinateTypeNotFound);
                }

                coordinate.MarkCoordinateType(hitTypeResult.Value);
            }
        }

        if (!isShipCoordinate && (coordinate.CoordinateType.Type.Equals("Empty") ||
                                  coordinate.CoordinateType.Type.Equals("Filled")))
        {
            var missedTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Missed");
            if (missedTypeResult.IsFailure)
            {
                return Result.Failure<Coordinate>(ServiceErrors.CoordinateTypeServiceExceptions.CoordinateTypeNotFound);
            }

            coordinate.MarkCoordinateType(missedTypeResult.Value);
        }

        await _coordinateService.Update(coordinate);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult
            ? Result.Success(coordinate)
            : Result.Failure<Coordinate>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result> UpdatePlayerTurn(int gameId, int coordinateId, int userId)
    {
        var game = await _repository.GetById(gameId);

        if (game is null)
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }

        var coordinateResult = await _coordinateService.GetById(coordinateId);

        if (coordinateResult.IsFailure)
        {
            return Result.Failure<Coordinate>(coordinateResult.Error);
        }

        var userGameCurrentPlayer = game.GameUsers.FirstOrDefault(gu => gu.AppUserId == userId)!;
        var userGameSecondPlayer = game.GameUsers.FirstOrDefault(gu => gu.AppUserId != userId)!;
        var isCurrentPlayerTurn = userGameCurrentPlayer.IsPlayerTurn;

        if (coordinateResult.Value.CoordinateType.Type.Equals("Missed"))
        {
            userGameCurrentPlayer.IsPlayerTurn = !isCurrentPlayerTurn;
            userGameSecondPlayer.IsPlayerTurn = isCurrentPlayerTurn;
        }
        else if (coordinateResult.Value.CoordinateType.Type.Equals("Hit") ||
                 coordinateResult.Value.CoordinateType.Type.Equals("Destroyed"))
        {
            userGameCurrentPlayer.IsPlayerTurn = isCurrentPlayerTurn;
            userGameSecondPlayer.IsPlayerTurn = !isCurrentPlayerTurn;
        }

        await _userGameService.Update(userGameCurrentPlayer);
        await _userGameService.Update(userGameSecondPlayer);

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult ? Result.Success() : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }
}