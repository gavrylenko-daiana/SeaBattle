namespace SeaBattle.Infrastructure.Exceptions;

public class DerivedInstanceException : OrmErrors
{
    public override string Message => $"Error: {nameof(DerivedInstanceException)}. Was not found the correct derived instance for type name.";
}