namespace SeaBattle.Infrastructure.Exceptions;

public class NullException : OrmErrors
{
    public override string Message => $"Error: {nameof(NullReferenceException)}. Such entity does not exists.";
}