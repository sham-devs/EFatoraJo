using EFatoraJoConsoleApp.Models;
using ShamDevs.EFatoraJo.Models.Responses;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Output;

/// <summary>
/// Handles output formatting for both JSON and text modes
/// </summary>
public static class OutputHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Write success response
    /// </summary>
    public static void WriteSuccess(OutputFormat format, string message, string? invoiceNumber = null,
        string? qrCode = null, string? returnInvoiceNumber = null)
    {
        if (format == OutputFormat.Json)
        {
            var response = new SuccessResponse
            {
                Message = message,
                InvoiceNumber = invoiceNumber,
                QrCode = qrCode,
                ReturnInvoiceNumber = returnInvoiceNumber
            };
            Console.WriteLine(JsonSerializer.Serialize(response, JsonOptions));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✓ {message}");
            if (!string.IsNullOrEmpty(invoiceNumber))
                Console.WriteLine($"  Invoice Number: {invoiceNumber}");
            if (!string.IsNullOrEmpty(qrCode))
                Console.WriteLine($"  QR Code: {qrCode}");
            if (!string.IsNullOrEmpty(returnInvoiceNumber))
                Console.WriteLine($"  Return Invoice Number: {returnInvoiceNumber}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Write error response and set exit code
    /// </summary>
    public static void WriteError(OutputFormat format, int exitCode, string message, List<ErrorDetail>? errors = null)
    {
        Environment.ExitCode = exitCode;

        if (format == OutputFormat.Json)
        {
            var response = new ErrorResponse
            {
                ErrorType = ExitCodes.GetErrorType(exitCode),
                Message = message,
                Errors = errors ?? new List<ErrorDetail>(),
                ExitCode = exitCode
            };
            Console.WriteLine(JsonSerializer.Serialize(response, JsonOptions));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ ERROR: {message}");
            Console.WriteLine($"  Exit Code: {exitCode} ({ExitCodes.GetErrorType(exitCode)})");

            if (errors != null && errors.Count > 0)
            {
                Console.WriteLine("\nDetails:");
                foreach (var error in errors)
                {
                    if (!string.IsNullOrEmpty(error.Field))
                        Console.Write($"  - {error.Field}: ");
                    else if (!string.IsNullOrEmpty(error.Path))
                        Console.Write($"  - {error.Path}: ");
                    else
                        Console.Write("  - ");

                    Console.WriteLine(error.Message);

                    if (!string.IsNullOrEmpty(error.ExpectedValue))
                        Console.WriteLine($"    Expected: {error.ExpectedValue}");
                    if (!string.IsNullOrEmpty(error.ActualValue))
                        Console.WriteLine($"    Actual: {error.ActualValue}");
                }
            }
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Write validation errors from InvoiceValidationException
    /// </summary>
    public static void WriteValidationErrors(OutputFormat format, string message, List<string> validationErrors)
    {
        var errors = validationErrors.Select(e => new ErrorDetail
        {
            Message = e
        }).ToList();

        WriteError(format, ExitCodes.ValidationError, message, errors);
    }

    /// <summary>
    /// Write JSON parsing error
    /// </summary>
    public static void WriteJsonParseError(OutputFormat format, string message, string? path = null, string? details = null)
    {
        var errors = new List<ErrorDetail>
        {
            new ErrorDetail
            {
                Path = path,
                Message = details ?? message
            }
        };

        WriteError(format, ExitCodes.JsonParseError, message, errors);
    }

    /// <summary>
    /// Write API error from EInvoiceResponse
    /// </summary>
    public static void WriteApiError(OutputFormat format, EInvoiceResponse response)
    {
        var errors = new List<ErrorDetail>();

        if (response.HasErrors())
        {
            var messages = response.Results?.Errors;

            if (messages != null)
            {
                foreach (var msg in messages)
                {
                    errors.Add(new ErrorDetail
                    {
                        Message = msg.Message ?? "Unknown error"
                    });
                }
            }
        }

        // Add warnings too
        if (response.HasWarnings())
        {
            var warnings = response.Results?.Warnings;

            if (warnings != null)
            {
                foreach (var warn in warnings)
                {
                    errors.Add(new ErrorDetail
                    {
                        Message = $"[WARNING] {warn.Message}"
                    });
                }
            }
        }

        WriteError(format, ExitCodes.ApiError, "Invoice submission failed", errors);
    }

    /// <summary>
    /// Write file not found error
    /// </summary>
    public static void WriteFileNotFoundError(OutputFormat format, string filePath)
    {
        var errors = new List<ErrorDetail>
        {
            new ErrorDetail
            {
                Field = "file",
                Message = $"File not found: {filePath}"
            }
        };

        WriteError(format, ExitCodes.FileNotFoundError, "File not found", errors);
    }

    /// <summary>
    /// Write configuration error
    /// </summary>
    public static void WriteConfigurationError(OutputFormat format, string message, List<string>? missingSecrets = null)
    {
        var errors = missingSecrets?.Select(secret => new ErrorDetail
        {
            Field = secret,
            Message = $"Missing or empty: {secret}"
        }).ToList();

        WriteError(format, ExitCodes.ConfigurationError, message, errors);
    }

    /// <summary>
    /// Write authentication error
    /// </summary>
    public static void WriteAuthenticationError(OutputFormat format)
    {
        WriteError(format, ExitCodes.AuthenticationError,
            "Authentication failed - please check your ClientId and SecretKey");
    }

    /// <summary>
    /// Write unexpected error
    /// </summary>
    public static void WriteUnexpectedError(OutputFormat format, Exception ex)
    {
        var errors = new List<ErrorDetail>
        {
            new ErrorDetail
            {
                Message = ex.Message
            }
        };

        WriteError(format, ExitCodes.UnexpectedError, "An unexpected error occurred", errors);
    }
}
