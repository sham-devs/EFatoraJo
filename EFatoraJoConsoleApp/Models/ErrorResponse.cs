using System.Text.Json.Serialization;

namespace EFatoraJoConsoleApp.Models;

/// <summary>
/// Represents a structured error response for machine-readable output
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<ErrorDetail> Errors { get; set; } = new();

    [JsonPropertyName("exitCode")]
    public int ExitCode { get; set; }
}

/// <summary>
/// Represents a single error detail
/// </summary>
public class ErrorDetail
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("expectedValue")]
    public string? ExpectedValue { get; set; }

    [JsonPropertyName("actualValue")]
    public string? ActualValue { get; set; }
}

/// <summary>
/// Represents a successful response
/// </summary>
public class SuccessResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("invoiceNumber")]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; }

    [JsonPropertyName("returnInvoiceNumber")]
    public string? ReturnInvoiceNumber { get; set; }

    [JsonPropertyName("exitCode")]
    public int ExitCode { get; set; } = 0;
}
