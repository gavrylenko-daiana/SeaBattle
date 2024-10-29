namespace SeaBattle.Domain.Models.Dto;

public class UserDto
{
    public int AppUserId { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public ICollection<UserGames> UserGames { get; set; }
    // public ICollection<GameInvitation> GameInvitations { get; set; }
}