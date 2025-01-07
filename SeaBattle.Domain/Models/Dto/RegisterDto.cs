using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Domain.Models.Dto;

public class RegisterDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; }
    [Required]
    [RegularExpression(@"^.{8,}$", ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
}