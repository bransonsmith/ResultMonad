namespace ResultMonad;

public abstract record Result<T>(string? Message)
{
    public string Message { get; init; } = Message ?? "No message provided.";
}

public sealed record ResultSuccess<T>(T Data, string? Message = null)
    : Result<T>(Message);

public sealed record ResultFailure<T>(string? Message, ResultErrorCode ErrorCode, Exception? Exception = null)
    : Result<T>(Message);
