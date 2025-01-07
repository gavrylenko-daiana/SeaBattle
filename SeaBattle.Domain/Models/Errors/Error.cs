namespace SeaBattle.Domain.Models.Errors;

public class Error
{
    public static readonly Error None = new(string.Empty, string.Empty);
    
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
    
    public string Code { get; private set; }
    
    public string Message { get; private set; }
}