namespace PersonManagement.Domain.Common;

/// <summary>
/// Padrão Result para tratamento elegante de erros e sucessos
/// </summary>
/// <typeparam name="T">Tipo do valor de retorno</typeparam>
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> Errors { get; init; } = new();
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, string? errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Cria um resultado de erro com mensagem única
    /// </summary>
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage, new List<string> { errorMessage });

    /// <summary>
    /// Cria um resultado de erro com múltiplas mensagens
    /// </summary>
    public static Result<T> Failure(List<string> errors) => new(false, default, string.Join("; ", errors), errors);

    /// <summary>
    /// Cria um resultado de erro a partir de uma exceção
    /// </summary>
    public static Result<T> Failure(Exception exception) => new(false, default, exception.Message, new List<string> { exception.Message });

    /// <summary>
    /// Converte implicitamente um valor para resultado de sucesso
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Converte implicitamente uma string para resultado de erro
    /// </summary>
    public static implicit operator Result<T>(string errorMessage) => Failure(errorMessage);
}

/// <summary>
/// Resultado sem valor de retorno (apenas sucesso/erro)
/// </summary>
public record Result
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> Errors { get; init; } = new();
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, string? errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Cria um resultado de erro com mensagem única
    /// </summary>
    public static Result Failure(string errorMessage) => new(false, errorMessage, new List<string> { errorMessage });

    /// <summary>
    /// Cria um resultado de erro com múltiplas mensagens
    /// </summary>
    public static Result Failure(List<string> errors) => new(false, string.Join("; ", errors), errors);

    /// <summary>
    /// Cria um resultado de erro a partir de uma exceção
    /// </summary>
    public static Result Failure(Exception exception) => new(false, exception.Message, new List<string> { exception.Message });

    /// <summary>
    /// Converte implicitamente uma string para resultado de erro
    /// </summary>
    public static implicit operator Result(string errorMessage) => Failure(errorMessage);

    /// <summary>
    /// Converte para resultado genérico
    /// </summary>
    public Result<T> To<T>() => IsSuccess ?
        Result<T>.Success(default(T)!) :
        Result<T>.Failure(Errors);
}