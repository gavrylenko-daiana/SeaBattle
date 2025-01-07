namespace SeaBattle.Application.Interfaces;

public interface IPasswordHasherService
{
    string GetPasswordHash(string input);
}