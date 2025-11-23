namespace EFatoraJoConsoleApp.Models;

/// <summary>
/// Standardized exit codes for the application
/// </summary>
public static class ExitCodes
{
    /// <summary>
    /// Success - Invoice submitted successfully
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Validation Error - Invoice failed business validation rules
    /// </summary>
    public const int ValidationError = 1;

    /// <summary>
    /// API Error - Error communicating with EFatora API
    /// </summary>
    public const int ApiError = 2;

    /// <summary>
    /// Authentication Error - Invalid credentials or unauthorized
    /// </summary>
    public const int AuthenticationError = 3;

    /// <summary>
    /// JSON Parse Error - Invalid JSON format or structure
    /// </summary>
    public const int JsonParseError = 4;

    /// <summary>
    /// Configuration Error - Missing or invalid configuration
    /// </summary>
    public const int ConfigurationError = 5;

    /// <summary>
    /// File Not Found Error - Specified file does not exist
    /// </summary>
    public const int FileNotFoundError = 6;

    /// <summary>
    /// Unexpected Error - Unhandled exception
    /// </summary>
    public const int UnexpectedError = 99;

    /// <summary>
    /// Get error type string from exit code
    /// </summary>
    public static string GetErrorType(int exitCode)
    {
        return exitCode switch
        {
            Success => "Success",
            ValidationError => "ValidationError",
            ApiError => "ApiError",
            AuthenticationError => "AuthenticationError",
            JsonParseError => "JsonParseError",
            ConfigurationError => "ConfigurationError",
            FileNotFoundError => "FileNotFoundError",
            UnexpectedError => "UnexpectedError",
            _ => "UnknownError"
        };
    }
}
