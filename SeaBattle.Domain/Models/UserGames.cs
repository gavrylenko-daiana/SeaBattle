using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Helpers;

namespace SeaBattle.Domain.Models;

[Table("UserGames")]
public class UserGames
{
    public UserGames() { }
    
    public UserGames(Game game, AppUser appUser, GameField gameField)
    {
        Game = game;
        GameId = game.GameId;
        AppUser = appUser;
        AppUserId = appUser.AppUserId;
        GameField = gameField;
        GameFieldId = gameField.GameFieldId;
        IsReady = false;
        IsPlayerTurn = false;
    }

    [Key]
    [Column("UserGamesId")]
    public int UserGamesId { get; set; }
    
    [Column("GameId")]
    public int GameId { get; set; }
    
    [ForeignKey("GameId")]
    public Game Game { get; set; }
    
    [Column("AppUserId")]
    public int AppUserId { get; set; }
    
    [TypeEntity]
    [ForeignKey("AppUserId")]
    public AppUser AppUser { get; set; }
    
    [Column("GameFieldId")]
    public int GameFieldId { get; set; }
    
    [ForeignKey("GameFieldId")]
    public GameField GameField { get; set; }
    
    [Column("IsReady")]
    public bool IsReady { get; set; }
    
    [Column("IsPlayerTurn")]
    public bool IsPlayerTurn { get; set; }
}