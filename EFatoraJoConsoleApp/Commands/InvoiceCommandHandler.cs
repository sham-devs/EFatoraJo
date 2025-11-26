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

    private static readonly JsonSerializerOptions JsonOutputOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public InvoiceCommandHandler(string clientId, string secretKey)
    {
        _clientId = clientId;
        _secretKey = secretKey;
    }

    /// <summary>
    /// Process a regular sales invoice from file
    /// </summary>
    public async Task<int> ProcessInvoiceCommand(string filePath)
    {
        try
        {
            // 1. Read and parse JSON file
            string json = await File.ReadAllTextAsync(filePath);
            Invoice invoice = InvoiceJsonParser.ParseInvoice(json);

            // 2. Call SDK directly (no overrides, no modifications)
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, _clientId, _secretKey);

            // 3. Process result
            if (response.IsSuccessfullySubmitted())
            {
                WriteSuccessOutput(response);
                return 0; // Success
            }

            // Handle already submitted invoices as success (they were previously accepted)
            if (response.IsAlreadySubmitted())
            {
                WriteAlreadySubmittedOutput(response);
                return 0; // Success - invoice was already submitted
            }

            WriteApiErrorOutput(response);
            return 2; // API Error
        }
        catch (FileNotFoundException)
        {
            WriteErrorOutput("FileNotFoundError",
                $"File not found: {filePath}",
                new List<string> { $"The file '{filePath}' does not exist" });
            return 6;
        }
        catch (JsonException ex)
        {
            WriteErrorOutput("JsonParseError",
                "Failed to parse invoice JSON",
                new List<string> { ex.Message });
            return 4;
        }
        catch (InvoiceValidationException ex)
        {
            WriteErrorOutput("InvoiceValidationException",
                "Invoice validation failed",
                ex.ValidationErrors?.ToList() ?? new List<string>());
            return 1;
        }
        catch (UblGenerationException ex)
        {
            WriteErrorOutput("UblGenerationException",
                "UBL document generation failed",
                new List<string> { ex.Message, ex.InnerException?.Message ?? "" });
            return 2;
        }
        catch (EInvoiceSerializationException ex)
        {
            WriteErrorOutput("EInvoiceSerializationException",
                "XML serialization failed",
                new List<string> { ex.Message });
            return 2;
        }
        catch (EInvoiceApiException ex)
        {
            var errors = ParseApiErrorResponse(ex.ResponseContent, ex.Message);
            WriteErrorOutput("EInvoiceApiException",
                $"API communication failed (HTTP {ex.StatusCode})",
                errors);
            return ex.StatusCode == 401 ? 3 : 2;
        }
        catch (EInvoiceException ex)
        {
            WriteErrorOutput("EInvoiceException",
                "Unexpected error during invoice processing",
                new List<string> { ex.Message, ex.InnerException?.Message ?? "" });
            return 99;
        }
        catch (Exception ex)
        {
            WriteErrorOutput("UnexpectedException",
                "An unexpected error occurred",
                new List<string> { ex.Message });
            return 99;
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
    public async Task<int> ProcessReturnInvoiceCommand(string filePath)
    {
        try
        {
            // 1. Read and parse JSON file containing return invoice data
            string json = await File.ReadAllTextAsync(filePath);

            // Parse JSON to check for returnUniqueSerialNumber field
            var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
            string? returnUuid = null;
            if (jsonDoc.RootElement.TryGetProperty("returnUniqueSerialNumber", out var returnUuidElement))
            {
                returnUuid = returnUuidElement.GetString();
            }

            // The JSON should contain the original invoice structure
            // We parse it as a regular invoice first to get the structure
            Invoice originalInvoice = InvoiceJsonParser.ParseInvoice(json);

            // 2. Generate new UUID for return invoice if not provided
            // The returnUniqueSerialNumber is for the RETURN invoice (new UUID)
            // The originalInvoice.UniqueSerialNumber is the ORIGINAL invoice UUID
            string returnInvoiceUuid = !string.IsNullOrEmpty(returnUuid)
                ? returnUuid
                : Guid.NewGuid().ToString();

            // 3. Create SalesReturnInvoice using the parsed data
            // Note: returnedInvoice contains the original invoice with its UUID
            // uniqueSerialNumber is the NEW UUID for the return invoice
            var returnInvoice = new SalesReturnInvoice(
                invoiceNumber: $"RET-{originalInvoice.InvoiceNumber}",
                returnedInvoice: originalInvoice,
                uniqueSerialNumber: returnInvoiceUuid,  // NEW UUID for return invoice
                invoiceDate: originalInvoice.InvoiceDate,
                returnReason: originalInvoice.InvoiceNote ?? "Product return"
            );

            // 3. Call SDK for return invoice
            var response = await EFatoraJoSdk.SendReturnFatoraAsync(returnInvoice, _clientId, _secretKey);

            // 4. Process result
            if (response.IsSuccessfullySubmitted())
            {
                WriteSuccessOutput(response);
                return 0; // Success
            }

            // Handle already submitted invoices as success (they were previously accepted)
            if (response.IsAlreadySubmitted())
            {
                WriteAlreadySubmittedOutput(response);
                return 0; // Success - invoice was already submitted
            }

            WriteApiErrorOutput(response);
            return 2; // API Error
        }
        catch (FileNotFoundException)
        {
            WriteErrorOutput("FileNotFoundError",
                $"File not found: {filePath}",
                new List<string> { $"The file '{filePath}' does not exist" });
            return 6;
        }
        catch (JsonException ex)
        {
            WriteErrorOutput("JsonParseError",
                "Failed to parse return invoice JSON",
                new List<string> { ex.Message });
            return 4;
        }
        catch (InvoiceValidationException ex)
        {
            WriteErrorOutput("InvoiceValidationException",
                "Return invoice validation failed",
                ex.ValidationErrors?.ToList() ?? new List<string>());
            return 1;
        }
        catch (UblGenerationException ex)
        {
            WriteErrorOutput("UblGenerationException",
                "Return UBL document generation failed",
                new List<string> { ex.Message, ex.InnerException?.Message ?? "" });
            return 2;
        }
        catch (EInvoiceSerializationException ex)
        {
            WriteErrorOutput("EInvoiceSerializationException",
                "XML serialization failed for return invoice",
                new List<string> { ex.Message });
            return 2;
        }
        catch (EInvoiceApiException ex)
        {
            var errors = ParseApiErrorResponse(ex.ResponseContent, ex.Message);
            WriteErrorOutput("EInvoiceApiException",
                $"API communication failed for return invoice (HTTP {ex.StatusCode})",
                errors);
            return ex.StatusCode == 401 ? 3 : 2;
        }
        catch (EInvoiceException ex)
        {
            WriteErrorOutput("EInvoiceException",
                "Unexpected error during return invoice processing",
                new List<string> { ex.Message, ex.InnerException?.Message ?? "" });
            return 99;
        }
        catch (Exception ex)
        {
            WriteErrorOutput("UnexpectedException",
                "An unexpected error occurred",
                new List<string> { ex.Message });
            return 99;
        }
    }

    /// <summary>
    /// Write success output in the required JSON format
    /// </summary>
    private void WriteSuccessOutput(EInvoiceResponse response)
    {
        var output = new
        {
            success = true,
            result = response
        };

        Console.WriteLine(JsonSerializer.Serialize(output, JsonOutputOptions));
    }

    /// <summary>
    /// Write error output in the required JSON format
    /// </summary>
    private void WriteErrorOutput(string errorType, string message, List<string> errors)
    {
        var output = new
        {
            success = false,
            errorType = errorType,
            message = message,
            errors = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList()
        };

        Console.WriteLine(JsonSerializer.Serialize(output, JsonOutputOptions));
    }

    /// <summary>
    /// Write output for already submitted invoices (treated as success)
    /// </summary>
    private void WriteAlreadySubmittedOutput(EInvoiceResponse response)
    {
        var output = new
        {
            success = true,
            alreadySubmitted = true,
            message = "Invoice was already submitted",
            result = response
        };

        Console.WriteLine(JsonSerializer.Serialize(output, JsonOutputOptions));
    }

    /// <summary>
    /// Write API error when response contains errors/warnings
    /// </summary>
    private void WriteApiErrorOutput(EInvoiceResponse response)
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

        WriteErrorOutput("EInvoiceApiException", "Invoice submission failed", errors);
    }
}
