using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Helpers;

namespace SeaBattle.Domain.Models;

[Table("Games")]
public class Game
{
    public Game() { }
    
    public Game(string name, int currentUserId)
    {
        Name = name;
        Progress = GameProgress.PlayerWaiting;
        CreatorId = currentUserId;
    } 
    
    [Key]
    [Column("GameId")]
    public int GameId { get; set; }
    
    [Column("Name")]
    public string Name { get; set; }
    
    [Column("Progress")]
    public GameProgress Progress { get; set; }
    
    [Column("CreatorId")]
    public int CreatorId { get; set; }

    [NotLoad]
    [JsonIgnore]
    public List<GameInvitation> GameInvitations { get; set; } = new List<GameInvitation>();
    
    public List<UserGames> GameUsers { get; set; } = new List<UserGames>();
}
