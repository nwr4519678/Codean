namespace Platform.Shared.Results;

/// <summary>
/// Discriminated-union result that lets us return success or a typed failure
/// from use-case handlers without throwing for expected business outcomes.
/// </summary>
public readonly struct Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public Error Error { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        Error = default!;
    }

    private Result(Error error)
    {
        Value = default;
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(Value!)) : Result<TOut>.Failure(Error);

    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder) =>
        IsSuccess ? await binder(Value!) : Result<TOut>.Failure(Error);
}

/// <summary>Non-generic result for commands that don't return a value.</summary>
public readonly struct Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    private Result(bool ok, Error error)
    {
        IsSuccess = ok;
        Error = error;
    }

    public static Result Success() => new(true, default!);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}
