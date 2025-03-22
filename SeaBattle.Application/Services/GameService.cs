using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Interfaces;
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
    private readonly IShipTypeService _shipTypeService;
    private readonly SeaBattle.Domain.Interfaces.ICoordinateService _computeCoordinateService;
    private readonly IComputeCoordinateService _computeCoordinate;
    private readonly IValidationService _validationService;

    public GameService(IRepository<Game> repository, IUnitOfWork unitOfWork, IUserGameService userGameService,
        IAppUserService userService, IGameInvitationService invitationService, IShipService shipService,
        ICoordinateService coordinateService,
        IShipCoordinateService shipCoordinateService, IGameFieldService gameFieldService,
        ICoordinateTypeService coordinateTypeService,
        IShipTypeService shipTypeService, SeaBattle.Domain.Interfaces.ICoordinateService computeCoordinateService,
        IComputeCoordinateService computeCoordinate, IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _userGameService = userGameService;
        _userService = userService;
        _invitationService = invitationService;
        _shipService = shipService;
        _coordinateService = coordinateService;
        _shipCoordinateService = shipCoordinateService;
        _gameFieldService = gameFieldService;
        _coordinateTypeService = coordinateTypeService;
        _shipTypeService = shipTypeService;
        _computeCoordinateService = computeCoordinateService;
        _computeCoordinate = computeCoordinate;
        _validationService = validationService;
        _repository = repository;
    }

    public async Task<Result<List<Game>>> GetAll(Expression<Func<Game, bool>> filter = null!,
        Expression<Func<IQueryable<Game>, IOrderedQueryable<Game>>> orderBy = null!, int pageNumber = 1,
        int pageSize = 1000)
    {
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>> include = query => query
            .Include(g => g.GameUsers)
            .ThenInclude(gu => gu.AppUser);

        var games = await _repository.GetAll(filter, orderBy, include);

        var paginatedGames = games.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return paginatedGames.Any()
            ? Result.Success(paginatedGames)
            : Result.Failure<List<Game>>(ServiceErrors.GameServiceExceptions.NonExistentGames);
    }

    public async Task<Result<Game>> GetById(int id)
    {
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>> include = query => query
            .Include(g => g.GameUsers)
            .ThenInclude(ug => ug.GameField)
            .ThenInclude(gf => gf.Coordinates)
            .ThenInclude(c => c.Point)
            .Include(g => g.GameUsers)
            .ThenInclude(ug => ug.GameField)
            .ThenInclude(gf => gf.Coordinates)
            .ThenInclude(c => c.CoordinateType)
            .Include(g => g.GameUsers)
            .ThenInclude(ug => ug.GameField)
            .ThenInclude(gf => gf.Coordinates)
            .ThenInclude(c => c.ShipCoordinates)
            .ThenInclude(sc => sc.Ship)
            .Include(g => g.GameUsers)
            .ThenInclude(gu => gu.AppUser);

        var game = await _repository.GetById(id, include);

        return game is null
            ? Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NonExistentGame)
            : Result.Success(game);
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

        var save = await _unitOfWork.SaveChanges();

        var userGameResult = await _userGameService.Insert(game, currentUserResult.Value);

        if (userGameResult.IsFailure)
        {
            return Result.Failure<Game>(userGameResult.Error);
        }

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult
            ? Result.Success(game)
            : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result> Update(int id, GameDto gameDto)
    {
        var createGameResult = new Game(gameDto.Name, gameDto.CreatorId!.Value)
        {
            GameId = id
        };

        await _repository.Update(createGameResult);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult
            ? Result.Success()
            : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result> UpdateGameProgress(int id, GameProgress gameProgress)
    {
        var game = await _repository.GetById(id);

        if (game is null)
        {
            return Result.Failure(ServiceErrors.GameServiceExceptions.NonExistentGame);
        }

        game.Progress = gameProgress;
        await _repository.Update(game);
        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult
            ? Result.Success()
            : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
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

        return saveResult
            ? Result.Success()
            : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
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

        return saveResult
            ? Result.Success(game)
            : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
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

        return saveResult
            ? Result.Success(invitationResult.Value)
            : Result.Failure<GameInvitation>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
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

        await _unitOfWork.SaveChanges();

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

        var result =
            await _shipCoordinateService.AddShipToGameField(ship, userGame.GameField, getCoordinateResult.Value,
                shipDto);

        if (result.IsFailure)
        {
            return Result.Failure<Game>(result.Error);
        }

        await _shipService.Update(ship);
        await _gameFieldService.Update(userGame.GameField);

        var saveResult = await _unitOfWork.SaveChanges();

        return saveResult
            ? Result.Success(getGameResult.Value)
            : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<Game>> PlaceShipsAutomatically(int gameId, int userId)
    {
        var getGameResult = await GetById(gameId);

        if (getGameResult.IsFailure)
        {
            return Result.Failure<Game>(getGameResult.Error);
        }

        var game = getGameResult.Value;
        var userGame = game.GameUsers.FirstOrDefault(gu => gu.AppUserId == userId);

        if (userGame?.GameField is null)
        {
            return Result.Failure<Game>(ServiceErrors.UserGameFieldServiceExceptions.NonExistentUserGame);
        }

        var gameField = userGame.GameField;

        await ClearShipsFromField(gameField);
        await _unitOfWork.SaveChanges();

        var shipSizes = new List<int> { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        int maxAttempts = 3; // Number of placement attempts per ship
        int maxResets = 5; // Number of times to reset the entire board and try again

        for (int resetCount = 0; resetCount < maxResets; resetCount++)
        {
            bool allShipsPlaced = true;

            if (resetCount > 0)
            {
                await ClearShipsFromField(gameField);
                await _unitOfWork.SaveChanges();
            }

            foreach (var size in shipSizes)
            {
                bool shipPlaced = false;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var placementResult = await TryPlaceShipBFS(size, gameField, userId, gameId);

                    if (!placementResult.IsFailure)
                    {
                        shipPlaced = true;
                        await _unitOfWork.SaveChanges();
                        break;
                    }
                }

                if (!shipPlaced)
                {
                    allShipsPlaced = false;
                    break;
                }
            }

            if (allShipsPlaced)
            {
                return Result.Success(game);
            }
        }

        await ClearShipsFromField(gameField);
        await _unitOfWork.SaveChanges();
        
        return Result.Failure<Game>(ServiceErrors.GameFieldServiceExceptions.NoValidPlacement);
    }

    private async Task ClearShipsFromField(GameField gameField)
    {
        var emptyTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Empty");
        if (emptyTypeResult.IsFailure)
        {
            return;
        }

        foreach (var coordinate in gameField.Coordinates)
        {
            coordinate.ShipCoordinates.Clear();

            coordinate.MarkCoordinateType(emptyTypeResult.Value);
        }

        await _unitOfWork.SaveChanges();
    }

    private async Task<Result> TryPlaceShipBFS(int shipSize, GameField gameField, int userId, int gameId)
    {
        var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();
        var random = new Random();

        var startCoordinates = gameField.Coordinates
            .Where(c => c.CoordinateType.Type == "Empty")
            .OrderBy(_ => random.Next())
            .ToList();

        var queue = new Queue<Coordinate>(startCoordinates);

        while (queue.Count > 0)
        {
            var startCoordinate = queue.Dequeue();

            foreach (var direction in directions.OrderBy(_ => random.Next()))
            {
                var shipTypeResult = await _shipTypeService.GetShipTypeByTypeNameAsync("war");
                
                if (shipTypeResult.IsFailure)
                {
                    continue;
                }

                var emptyTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Empty");
                if (emptyTypeResult.IsFailure)
                {
                    continue;
                }

                var currentCoordinate = startCoordinate;
                var shipCoordinates = new List<Coordinate>();
                bool validPlacement = true;

                for (int i = 0; i < shipSize; i++)
                {
                    if (currentCoordinate == null ||
                        !_validationService.IsValidPoint(currentCoordinate.Point) ||
                        currentCoordinate.CoordinateType.Type != "Empty")
                    {
                        validPlacement = false;
                        break;
                    }

                    shipCoordinates.Add(currentCoordinate);
                    currentCoordinate = _computeCoordinate.GetNextCoordinate(currentCoordinate, direction, gameField);
                }

                if (!validPlacement)
                {
                    continue;
                }

                var shipDto = new ShipDto
                {
                    GameId = gameId,
                    CoordinateId = startCoordinate.CoordinateId,
                    Size = shipSize,
                    Direction = direction.ToString()
                };

                var addShipResult = await AddShipToField(shipDto, userId);
                
                if (!addShipResult.IsFailure)
                {
                    return Result.Success();
                }
            }
        }

        return Result.Failure(ServiceErrors.GameFieldServiceExceptions.NoValidPlacement);
    }

    public async Task<Result<Game>> UpdateUserStatusOnReady(int gameId, int userId)
    {
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>> include = query => query
            .Include(g => g.GameUsers)
            .ThenInclude(ug => ug.GameField)
            .ThenInclude(gf => gf.Coordinates)
            .ThenInclude(c => c.Point)
            .Include(g => g.GameUsers)
            .ThenInclude(ug => ug.GameField)
            .ThenInclude(gf => gf.Coordinates)
            .ThenInclude(c => c.CoordinateType)
            .Include(g => g.GameUsers)
            .ThenInclude(ug => ug.GameField)
            .ThenInclude(gf => gf.Coordinates)
            .ThenInclude(c => c.ShipCoordinates)
            .ThenInclude(sc => sc.Ship);

        var game = await _repository.GetById(gameId, include);

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

        return saveResult
            ? Result.Success(game)
            : Result.Failure<Game>(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
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
                var destroyedTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Destroyed");

                if (destroyedTypeResult.IsFailure)
                {
                    return Result.Failure<Coordinate>(ServiceErrors.CoordinateTypeServiceExceptions
                        .CoordinateTypeNotFound);
                }

                coordinate.MarkCoordinateType(destroyedTypeResult.Value);
            }
            else
            {
                var hitTypeResult = await _coordinateTypeService.GetCoordinateTypeByTypeNameAsync("Hit");
                if (hitTypeResult.IsFailure)
                {
                    return Result.Failure<Coordinate>(ServiceErrors.CoordinateTypeServiceExceptions
                        .CoordinateTypeNotFound);
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
        Func<IQueryable<Game>, IIncludableQueryable<Game, object>> include = query => query
            .Include(g => g.GameUsers)
            .ThenInclude(gu => gu.AppUser);

        var game = await _repository.GetById(gameId, include);

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

        return saveResult
            ? Result.Success()
            : Result.Failure(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges);
    }

    public async Task<Result<Game>> FindGame(int userId)
    {
        const int ratingRange = 200;
        const int maxWaitTimeInSeconds = 300; // 5 minutes
        const int checkIntervalInMilliseconds = 1000; // 1 second

        var userResult = await _userService.GetUserById(userId);
        if (userResult.IsFailure)
        {
            return Result.Failure<Game>(userResult.Error);
        }

        var user = userResult.Value;
        user.Status = AppUserStatus.SearchingForGame;

        try
        {
            var userRating = user.Rating;
            var startTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - startTime).TotalSeconds < maxWaitTimeInSeconds)
            {
                var games = await _repository.GetAll(
                    filter: g => g.Progress == GameProgress.PlayerWaiting &&
                                 g.CreatorId != userId &&
                                 g.GameUsers.Any(gu => Math.Abs(gu.AppUser.Rating - userRating) <= ratingRange),
                    include: query => query.Include(g => g.GameUsers)
                        .ThenInclude(gu => gu.AppUser)
                );

                var suitableGame = games.FirstOrDefault();
                if (suitableGame != null)
                {
                    return Result.Success(suitableGame);
                }

                await Task.Delay(checkIntervalInMilliseconds);
            }

            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NoSuitableGame);
        }
        finally
        {
            user.Status = AppUserStatus.Idle;
            await _userService.UpdateUser(user);
        }
    }


    public async Task<Result<Game>> FindOpponent(int gameId)
    {
        const int ratingRange = 200;
        const int maxWaitTimeInSeconds = 300; // 5 minutes
        const int checkIntervalInMilliseconds = 1000; // 1 second

        var gameResult = await GetById(gameId);
        if (gameResult.IsFailure)
        {
            return Result.Failure<Game>(gameResult.Error);
        }

        var game = gameResult.Value;
        var userRating = game.GameUsers.FirstOrDefault()?.AppUser?.Rating;

        if (userRating == null)
        {
            return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.UnableToRetrieveUserRating);
        }

        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalSeconds < maxWaitTimeInSeconds)
        {
            var potentialOpponent = game.GameUsers.FirstOrDefault(gu =>
                gu.AppUserId != game.CreatorId &&
                gu.AppUser.Status == AppUserStatus.SearchingForGame &&
                Math.Abs(gu.AppUser.Rating - userRating.Value) <= ratingRange);

            if (potentialOpponent != null)
            {
                return Result.Success(game);
            }

            await Task.Delay(checkIntervalInMilliseconds);
        }

        return Result.Failure<Game>(ServiceErrors.GameServiceExceptions.NoSuitableOpponent);
    }
}