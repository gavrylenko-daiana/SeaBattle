using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaBattle.Domain.Models;

[Table("Users")]
public class AppUser
{
    [Key]
    [Column("AppUserId")]
    public int AppUserId { get; set; }
    
    [Column("FirstName")]
    public string FirstName { get; set; }
    
    [Column("LastName")]
    public string LastName { get; set; }
    
    [Column("UserName")]
    public string UserName { get; set; }
    
    [Column("Email")]
    public string Email { get; set; }
    
    [Column("PasswordHash")]
    public string PasswordHash { get; set; }
    
    public List<UserGames> UserGames { get; set; } = new List<UserGames>();
}