using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Models;

namespace SeaBattle.API.Hubs;

public class PlayHub : Hub
{
    private static readonly Dictionary<int, string> UserConnections = new();
    
    private readonly IAppUserService _userService;
    private readonly IGameService _gameService;
    private readonly IRepository<Game> _gameRepository;

    public PlayHub(IAppUserService userService, IGameService gameService, IRepository<Game> gameRepository)
    {
        _userService = userService;
        _gameService = gameService;
        _gameRepository = gameRepository;
    }
    
    public override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null && int.TryParse(httpContext.Request.Query["userId"], out var userId))
        {
            lock (UserConnections)
            {
                UserConnections[userId] = Context.ConnectionId;
            }
        }

        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null && int.TryParse(httpContext.Request.Query["userId"], out var userId))
        {
            lock (UserConnections)
            {
                UserConnections.Remove(userId);
            }
        }

        return base.OnDisconnectedAsync(exception);
    }
    
    public async Task FindGame(int userId)
    {
        const int ratingRange = 200;

        var userResult = await _userService.GetUserById(userId);
        if (userResult.IsFailure)
        {
            await Clients.Caller.SendAsync("SearchFailed", "User not found.");
            return;
        }

        var user = userResult.Value;
        user.Status = AppUserStatus.SearchingForGame;
        await _userService.UpdateUser(user);

        try
        {
            var userRating = user.Rating;
            while (true)
            {
                var games = await _gameRepository.GetAll(
                    filter: g => g.Progress == GameProgress.FindingOpponent &&
                                 g.CreatorId != userId &&
                                 g.GameUsers.Any(gu => Math.Abs(gu.AppUser.Rating - userRating) <= ratingRange),
                    include: query => query.Include(g => g.GameUsers)
                        .ThenInclude(gu => gu.AppUser)
                );

                var game = games.FirstOrDefault();
                if (game != null)
                {
                    var newGameResult = await _gameService.Join(game.GameId, userId);
                    var newGame = newGameResult.Value;
                    
                    await Clients.Caller.SendAsync("GameFound", newGame);
                    
                    var opponent = newGame.GameUsers.FirstOrDefault(gu => gu.AppUserId != userId);
                    if (opponent?.AppUser != null && UserConnections.TryGetValue(opponent.AppUserId, out var connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("OpponentFound", game);
                    }

                    break;
                }

                await Task.Delay(1000);
            }
        }
        finally
        {
            user.Status = AppUserStatus.Idle;
            await _userService.UpdateUser(user);
        }
    }

    public async Task FindOpponent(int gameId, int userId)
    {
        const int ratingRange = 200;

        var gameResult = await _gameService.GetById(gameId);
        if (gameResult.IsFailure)
        {
            await Clients.Caller.SendAsync("SearchFailed", "Game not found.");
            return;
        }

        var game = gameResult.Value;

        var userRating = game.GameUsers.FirstOrDefault()?.AppUser?.Rating;
        if (userRating == null)
        {
            await Clients.Caller.SendAsync("SearchFailed", "User not found.");
            return;
        }
        
        await _gameService.UpdateGameProgress(game.GameId, GameProgress.FindingOpponent);

        try
        {
            while (true)
            {
                var opponent = game.GameUsers.FirstOrDefault(gu =>
                    gu.AppUserId != game.CreatorId &&
                    gu.AppUser.Status == AppUserStatus.SearchingForGame && 
                    Math.Abs(gu.AppUser.Rating - userRating.Value) <= ratingRange);

                if (opponent != null)
                {
                    await Clients.Caller.SendAsync("OpponentFound", game);
                    
                    // Notify the opponent
                    // var searchingUser = game.GameUsers.FirstOrDefault(gu => gu.AppUserId == game.CreatorId);
                    // if (searchingUser?.AppUser != null)
                    // {
                    //     await Clients.User(searchingUser.AppUserId.ToString()).SendAsync("GameFound", game);
                    // }
                    
                    break;
                }

                await Task.Delay(1000);
            }
        }
        finally
        {
            await _gameService.UpdateGameProgress(game.GameId, GameProgress.PlayerWaiting);
        }
    }
}