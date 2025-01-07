namespace SeaBattle.Infrastructure.Exceptions;

public abstract class OrmErrors : Exception
{
    public abstract string Message { get; }
}