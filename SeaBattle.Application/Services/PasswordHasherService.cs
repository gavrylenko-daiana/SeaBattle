using System.Security.Cryptography;
using System.Text;
using SeaBattle.Application.Interfaces;

namespace SeaBattle.Application.Services;

public class PasswordHasherService : IPasswordHasherService
{
    public string GetPasswordHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            var builder = new StringBuilder();
            
            foreach (var b in hash)
            {
                builder.Append(b.ToString("x2"));
            }
            
            return builder.ToString();
        }
    }
}