using SeaBattle.Domain.Models.Errors;

namespace SeaBattle.Domain.Models.Results;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException();
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    private bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
    
    public Error Error { get; }

    public static Result Success() => new Result(true, Errors.Error.None);
    
    public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value, true, Errors.Error.None);
    
    public static Result Failure(Error error) => new Result(false, error);
    
    public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(bool isSuccess, Error error) : base(isSuccess, error)
    {
    }

    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsFailure
        ? throw new InvalidOperationException("The value of failure result cannot be accessed.")
        : _value!;
}