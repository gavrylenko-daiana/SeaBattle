using Microsoft.AspNetCore.SignalR;
using SeaBattle.API.Hubs.Interfaces;
using SeaBattle.Application.Interfaces;

namespace SeaBattle.API.Hubs.Services;

public class HubService : IHubService
{
    private readonly IHubContext<PlayHub> _playHubContext;

    public HubService(IHubContext<PlayHub> playHubContext)
    {
        _playHubContext = playHubContext;
    }
    
    public async Task CoordinateUpdated(int gameId)
    {
        await _playHubContext.Clients.All.SendAsync("ReceiveCoordinateUpdate", gameId);
    }

    public async Task UpdateGameList()
    {
        await _playHubContext.Clients.All.SendAsync("ReceiveGameListUpdate");
    }
    
    public async Task NotifyGameFound(int userId, object game)
    {
        await _playHubContext.Clients.User(userId.ToString()).SendAsync("GameFound", game);
    }

    public async Task NotifyOpponentFound(int userId, object opponent)
    {
        await _playHubContext.Clients.User(userId.ToString()).SendAsync("OpponentFound", opponent);
    }

    public async Task NotifyFailure(int userId, string message)
    {
        await _playHubContext.Clients.User(userId.ToString()).SendAsync("SearchFailed", message);
    }
}