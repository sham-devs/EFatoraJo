namespace EFatoraJoConsoleApp.Models;

/// <summary>
/// Standard command result used by all handlers and formatters.
/// </summary>
public class CommandResult
{
    public bool Success { get; init; }
    public int ExitCode { get; init; }
    public string? ErrorType { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public object? Result { get; init; }
    public bool AlreadySubmitted { get; init; }

    public static CommandResult SuccessResult(object? result, string? message = null, bool alreadySubmitted = false, int exitCode = ExitCodes.Success)
    {
        return new CommandResult
        {
            Success = true,
            ExitCode = exitCode,
            Result = result,
            Message = message,
            AlreadySubmitted = alreadySubmitted
        };
    }

    public static CommandResult ErrorResult(int exitCode, string errorType, string message, IEnumerable<string>? errors = null, object? result = null)
    {
        return new CommandResult
        {
            Success = false,
            ExitCode = exitCode,
            ErrorType = errorType,
            Message = message,
            Errors = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray() ?? Array.Empty<string>(),
            Result = result
        };
    }
}
