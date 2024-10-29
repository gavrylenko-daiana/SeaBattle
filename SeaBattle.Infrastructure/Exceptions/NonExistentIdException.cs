namespace SeaBattle.Infrastructure.Exceptions;

public class NonExistentIdException : OrmErrors
{
    public override string Message => $"Error: {nameof(NonExistentIdException)}. Such Id does not exists.";
}