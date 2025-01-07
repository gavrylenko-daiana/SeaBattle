namespace SeaBattle.API.Hubs.Interfaces;

public interface IHubService
{
    Task CoordinateUpdated(int gameId);
    Task UpdateGameList();
}