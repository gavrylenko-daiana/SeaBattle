namespace SeaBattle.Infrastructure.Exceptions;

public class KeyPropertyException : OrmErrors
{
    public override string Message => $"Error: {nameof(KeyPropertyException)}. No key property defined for the entity.";
}