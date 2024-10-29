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
}