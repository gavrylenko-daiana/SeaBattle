using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Domain.Models.Dto;

public class LoginDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; }
    [Required]
    [RegularExpression(@"^.{8,}$", ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; }
}