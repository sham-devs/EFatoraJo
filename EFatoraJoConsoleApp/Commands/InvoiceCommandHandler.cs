using EFatoraJoConsoleApp.Models;
using EFatoraJoConsoleApp.Serialization;
using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Models.Responses;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Commands;

/// <summary>
/// Handles invoice submission commands for both regular and return invoices
/// </summary>
public class InvoiceCommandHandler
{
    private readonly string _clientId;
    private readonly string _secretKey;

    public InvoiceCommandHandler(string clientId, string secretKey)
    {
        _clientId = clientId;
        _secretKey = secretKey;
    }

    /// <summary>
    /// Process a regular sales invoice from file
    /// </summary>
    public async Task<CommandResult> ProcessInvoiceCommand(string filePath)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            string json = await File.ReadAllTextAsync(filePath);
            Invoice invoice = InvoiceJsonParser.ParseInvoice(json);
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, _clientId, _secretKey);
            return BuildResultFromResponse(response, "Invoice submitted successfully");
        }, filePath, "invoice");
    }

    /// <summary>
    /// Process an invoice object directly (for interactive mode)
    /// </summary>
    public async Task<CommandResult> ProcessInvoice(Invoice invoice)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, _clientId, _secretKey);
            return BuildResultFromResponse(response, "Invoice submitted successfully");
        }, "in-memory", "invoice");
    }

    /// <summary>
    /// Process a return invoice object directly (for interactive mode)
    /// </summary>
    public async Task<CommandResult> ProcessReturnInvoice(SalesReturnInvoice returnInvoice)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            var response = await EFatoraJoSdk.SendReturnFatoraAsync(returnInvoice, _clientId, _secretKey);
            return BuildResultFromResponse(response, "Return invoice submitted successfully");
        }, "in-memory", "return invoice");
    }

    /// <summary>
    /// Build result from API response
    /// </summary>
    private static CommandResult BuildResultFromResponse(EInvoiceResponse response, string successMessage)
    {
        if (response.IsSuccessfullySubmitted())
        {
            return CommandResult.SuccessResult(response, successMessage);
        }

        if (response.IsAlreadySubmitted())
        {
            return CommandResult.SuccessResult(response,
                successMessage.Replace("submitted successfully", "was already submitted"),
                alreadySubmitted: true);
        }

        return CreateApiErrorResult(response);
    }

    /// <summary>
    /// Executes an operation with unified error handling for all invoice types
    /// </summary>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="filePath">File path for error messages (use "in-memory" for object-based operations)</param>
    /// <param name="operationType">Type description for error messages ("invoice" or "return invoice")</param>
    private async Task<CommandResult> ExecuteWithErrorHandling(
        Func<Task<CommandResult>> operation,
        string filePath,
        string operationType)
    {
        try
        {
            return await operation();
        }
        catch (FileNotFoundException)
        {
            return CommandResult.ErrorResult(
                ExitCodes.FileNotFoundError,
                "FileNotFoundError",
                $"File not found: {filePath}",
                new List<string> { $"The file '{filePath}' does not exist" });
        }
        catch (JsonException ex)
        {
            return CommandResult.ErrorResult(
                ExitCodes.JsonParseError,
                "JsonParseError",
                $"Failed to parse {operationType} JSON",
                new List<string> { ex.Message });
        }
        catch (InvoiceValidationException ex)
        {
            return CommandResult.ErrorResult(
                ExitCodes.ValidationError,
                "InvoiceValidationException",
                $"{char.ToUpper(operationType[0])}{operationType[1..]} validation failed",
                ex.ValidationErrors?.ToList() ?? new List<string>());
        }
        catch (UblGenerationException ex)
        {
            return CommandResult.ErrorResult(
                ExitCodes.ApiError,
                "UblGenerationException",
                $"UBL document generation failed for {operationType}",
                new List<string> { ex.Message, ex.InnerException?.Message ?? "" });
        }
        catch (EInvoiceSerializationException ex)
        {
            return CommandResult.ErrorResult(
                ExitCodes.ApiError,
                "EInvoiceSerializationException",
                $"XML serialization failed for {operationType}",
                new List<string> { ex.Message });
        }
        catch (EInvoiceApiException ex)
        {
            var errors = ParseApiErrorResponse(ex.ResponseContent, ex.Message);
            return CommandResult.ErrorResult(
                ex.StatusCode == 401 ? ExitCodes.AuthenticationError : ExitCodes.ApiError,
                "EInvoiceApiException",
                $"API communication failed for {operationType} (HTTP {ex.StatusCode})",
                errors);
        }
        catch (EInvoiceException ex)
        {
            return CommandResult.ErrorResult(
                ExitCodes.UnexpectedError,
                "EInvoiceException",
                $"Unexpected error during {operationType} processing",
                new List<string> { ex.Message, ex.InnerException?.Message ?? "" });
        }
        catch (Exception ex)
        {
            return CommandResult.ErrorResult(
                ExitCodes.UnexpectedError,
                "UnexpectedException",
                "An unexpected error occurred",
                new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Parse API error response JSON to extract readable error messages
    /// </summary>
    private List<string> ParseApiErrorResponse(string? responseContent, string fallbackMessage)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(responseContent))
        {
            errors.Add(fallbackMessage);
            return errors;
        }

        try
        {
            // Try to parse as JoFotara API response
            var response = JsonSerializer.Deserialize<EInvoiceResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response != null)
            {
                // Add status info
                errors.Add($"API Status: {response.Status}");
                if (response.Results != null)
                {
                    errors.Add($"Processing Status: {response.Results.Status}");
                }

                // Extract error messages
                if (response.Results?.Errors?.Count > 0)
                {
                    foreach (var err in response.Results.Errors)
                    {
                        var msg = err.ToFormattedString();
                        if (!string.IsNullOrWhiteSpace(msg))
                            errors.Add($"[ERROR] {msg}");
                    }
                }

                // Extract warnings
                if (response.Results?.Warnings?.Count > 0)
                {
                    foreach (var warn in response.Results.Warnings)
                    {
                        var msg = warn.ToFormattedString();
                        if (!string.IsNullOrWhiteSpace(msg))
                            errors.Add($"[WARNING] {msg}");
                    }
                }

                // If we got meaningful errors, return them
                if (errors.Count > 2)
                    return errors;
            }
        }
        catch
        {
            // If parsing fails, fall through to raw response
        }

        // If parsing failed or no errors found, return raw response
        errors.Clear();
        errors.Add(responseContent);
        return errors;
    }

    /// <summary>
    /// Process a return invoice from file
    /// </summary>
    public async Task<CommandResult> ProcessReturnInvoiceCommand(string filePath)
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            // 1. Read and parse JSON file containing return invoice data
            string json = await File.ReadAllTextAsync(filePath);
            var returnInput = ReturnInvoiceJsonParser.Parse(json);

            // 2. Use provided return UUID or generate a new one
            var returnInvoiceUuid = string.IsNullOrWhiteSpace(returnInput.ReturnUUID)
                ? Guid.NewGuid().ToString()
                : returnInput.ReturnUUID!;

            // 3. Create SalesReturnInvoice using the parsed data
            var returnInvoice = new SalesReturnInvoice(
                invoiceNumber: returnInput.ReturnInvoiceNumber,
                returnedInvoice: returnInput.OriginalInvoice,
                uniqueSerialNumber: returnInvoiceUuid,
                invoiceDate: returnInput.ReturnInvoiceDate,
                returnReason: returnInput.ReturnReason
            );

            // 4. Call SDK for return invoice
            var response = await EFatoraJoSdk.SendReturnFatoraAsync(returnInvoice, _clientId, _secretKey);
            return BuildResultFromResponse(response, "Return invoice submitted successfully");
        }, filePath, "return invoice");
    }

    /// <summary>
    /// Build API error result when response contains errors/warnings
    /// </summary>
    internal static CommandResult CreateApiErrorResult(EInvoiceResponse response)
    {
        var errors = new List<string>();

        // Add status information with clear labels
        errors.Add($"API Status: {response.Status}");
        if (response.Results != null)
        {
            errors.Add($"Processing Status: {response.Results.Status}");
        }

        // Add invoice number and UUID if available (helps with debugging)
        if (!string.IsNullOrEmpty(response.InvoiceNumber))
        {
            errors.Add($"Invoice Number: {response.InvoiceNumber}");
        }
        if (!string.IsNullOrEmpty(response.InvoiceUuid))
        {
            errors.Add($"Invoice UUID: {response.InvoiceUuid}");
        }

        // Add error messages using ToFormattedString() for full details
        if (response.HasErrors())
        {
            var messages = response.Results?.Errors;
            if (messages != null)
            {
                foreach (var msg in messages)
                {
                    var formatted = msg.ToFormattedString();
                    if (!string.IsNullOrWhiteSpace(formatted))
                        errors.Add($"[ERROR] {formatted}");
                }
            }
        }

        // Add warning messages using ToFormattedString() for full details
        if (response.HasWarnings())
        {
            var warnings = response.Results?.Warnings;
            if (warnings != null)
            {
                foreach (var warn in warnings)
                {
                    var formatted = warn.ToFormattedString();
                    if (!string.IsNullOrWhiteSpace(formatted))
                        errors.Add($"[WARNING] {formatted}");
                }
            }
        }

        // Add info messages that might contain useful details
        if (response.Results?.Info?.Count > 0)
        {
            foreach (var info in response.Results.Info)
            {
                var formatted = info.ToFormattedString();
                if (!string.IsNullOrWhiteSpace(formatted))
                    errors.Add($"[INFO] {formatted}");
            }
        }

        // If no specific errors/warnings/info, provide helpful guidance
        if (!response.HasErrors() && !response.HasWarnings() && (response.Results?.Info?.Count ?? 0) == 0)
        {
            errors.Add("No specific error details returned from JoFotara API");

            // Add guidance based on status
            if (response.Status == ShamDevs.EFatoraJo.Enums.EInvoiceStatus.REJECTED)
            {
                errors.Add("Invoice was REJECTED - check UUID matches a previously submitted invoice");
            }
            else if (response.Results?.Status == ShamDevs.EFatoraJo.Enums.EInvoiceProcessingStatus.ERROR)
            {
                errors.Add("Processing FAILED - the original invoice UUID may not exist in JoFotara system");
            }
            else
            {
                errors.Add("This may indicate: invalid UUID, invoice not found, or data mismatch");
            }
        }

        return CommandResult.ErrorResult(
            ExitCodes.ApiError,
            "EInvoiceApiException",
            "Invoice submission failed",
            errors,
            response);
    }
}
