using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SeaBattle.Domain.Helpers;

namespace SeaBattle.Domain.Models;

[Table("GameInvitations")]
public class GameInvitation
{
    public GameInvitation() { }

    public GameInvitation(Game game, AppUser appUser)
    {
        Game = game;
        GameId = game.GameId;
        AppUser = appUser;
        AppUserId = appUser.AppUserId;
    }
    
    [Key]
    [Column("GameInvitationId")]
    public int GameInvitationId { get; set; }
    
    [Column("GameId")]
    public int GameId { get; set; }
    
    [TypeEntity]
    [ForeignKey("GameId")]
    public Game Game { get; set; }
    
    [Column("AppUserId")]
    public int AppUserId { get; set; }
    
    [TypeEntity]
    [ForeignKey("AppUserId")]
    public AppUser AppUser { get; set; }
}