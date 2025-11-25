using EFatoraJoConsoleApp.Models;
using EFatoraJoConsoleApp.Output;
using EFatoraJoConsoleApp.Serialization;
using Microsoft.Extensions.Configuration;
using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Models;
using System.Text.Json;
using static EFatoraJoConsoleApp.Output.OutputHandler;

namespace EFatoraJoConsoleApp.Commands;

/// <summary>
/// Handles invoice submission commands from JSON input
/// </summary>
public class InvoiceCommandHandler
{
    private readonly string _clientId;
    private readonly string _secretKey;
    private readonly Supplier _supplierInfo;
    private readonly OutputFormat _outputFormat;

    public InvoiceCommandHandler(string clientId, string secretKey, Supplier supplierInfo, OutputFormat outputFormat)
    {
        _clientId = clientId;
        _secretKey = secretKey;
        _supplierInfo = supplierInfo;
        _outputFormat = outputFormat;
    }

    /// <summary>
    /// Load configuration from user secrets
    /// </summary>
    public static (string clientId, string secretKey, Supplier supplier, List<string> missingSecrets) LoadConfiguration()
    {
        var cfg = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string clientId = cfg["ClientId"] ?? string.Empty;
        string secretKey = cfg["SecretKey"] ?? string.Empty;

        var supplierCfg = cfg.GetSection("Supplier");
        string taxVATNumber = supplierCfg["TaxVATNumber"] ?? string.Empty;
        string incomeSourceSequence = supplierCfg["IncomeSourceSequence"] ?? string.Empty;
        string registeredSupplierName = supplierCfg["RegisteredSupplierName"] ?? string.Empty;

        var missingSecrets = new List<string>();
        if (string.IsNullOrWhiteSpace(clientId)) missingSecrets.Add("ClientId");
        if (string.IsNullOrWhiteSpace(secretKey)) missingSecrets.Add("SecretKey");
        if (string.IsNullOrWhiteSpace(taxVATNumber)) missingSecrets.Add("Supplier:TaxVATNumber");
        if (string.IsNullOrWhiteSpace(incomeSourceSequence)) missingSecrets.Add("Supplier:IncomeSourceSequence");
        if (string.IsNullOrWhiteSpace(registeredSupplierName)) missingSecrets.Add("Supplier:RegisteredSupplierName");

        var supplier = new Supplier(taxVATNumber, incomeSourceSequence, registeredSupplierName);

        return (clientId, secretKey, supplier, missingSecrets);
    }

    /// <summary>
    /// Process invoice from JSON string
    /// </summary>
    public async Task<int> ProcessInvoiceAsync(string json, string? returnJson = null)
    {
        try
        {
            // Parse invoice JSON
            Invoice invoice;
            try
            {
                invoice = InvoiceJsonParser.ParseInvoice(json);
            }
            catch (JsonException ex)
            {
                OutputHandler.WriteJsonParseError(_outputFormat, "Failed to parse invoice JSON", details: ex.Message);
                return ExitCodes.JsonParseError;
            }

            // Override supplier from configuration
            invoice = new Invoice(
                invoiceNumber: invoice.InvoiceNumber,
                uniqueSerialNumber: invoice.UniqueSerialNumber,
                invoiceDate: invoice.InvoiceDate,
                paymentType: invoice.PaymentType,
                supplier: _supplierInfo, // Use supplier from config
                customer: invoice.Customer,
                invoiceTotals: invoice.InvoiceTotals,
                invoiceDetails: invoice.InvoiceDetails,
                type: invoice.Type
            )
            {
                Currency = invoice.Currency,
                InvoiceNote = invoice.InvoiceNote
            };

            // If return invoice is provided, skip sending original invoice
            // and use it only as reference data for the return invoice
            if (!string.IsNullOrWhiteSpace(returnJson))
            {
                // Validate minimal required fields from original invoice
                if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
                {
                    OutputHandler.WriteError(_outputFormat, ExitCodes.ValidationError,
                        "Original invoice validation failed",
                        new List<ErrorDetail>
                        {
                            new ErrorDetail { Message = "Original invoice must have invoiceNumber for return reference" }
                        });
                    return ExitCodes.ValidationError;
                }

                if (string.IsNullOrWhiteSpace(invoice.UniqueSerialNumber))
                {
                    OutputHandler.WriteError(_outputFormat, ExitCodes.ValidationError,
                        "Original invoice validation failed",
                        new List<ErrorDetail>
                        {
                            new ErrorDetail { Message = "Original invoice must have uniqueSerialNumber for return reference" }
                        });
                    return ExitCodes.ValidationError;
                }

                // Process return invoice directly using original invoice as reference only
                var returnResponse = await ProcessReturnInvoiceAsync(returnJson, invoice);
                if (returnResponse != null && returnResponse.IsSuccessfullySubmitted())
                {
                    OutputHandler.WriteSuccess(_outputFormat,
                        $"Return invoice submitted successfully. Original invoice {invoice.InvoiceNumber} was used as reference only (not sent to system).",
                        invoiceNumber: returnResponse.InvoiceNumber,
                        qrCode: returnResponse.Qr);
                    return ExitCodes.Success;
                }

                return Environment.ExitCode; // Error already written by ProcessReturnInvoiceAsync
            }

            // Submit invoice (only when no return invoice is provided)
            var response = await SubmitInvoiceAsync(invoice);
            if (response == null)
            {
                return Environment.ExitCode; // Error already written
            }

            OutputHandler.WriteSuccess(_outputFormat,
                "Invoice submitted successfully",
                invoiceNumber: response.InvoiceNumber,
                qrCode: response.Qr);

            return ExitCodes.Success;
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            OutputHandler.WriteUnexpectedError(_outputFormat, ex);
            return ExitCodes.UnexpectedError;
        }
    }

    /// <summary>
    /// Process invoice from file
    /// </summary>
    public async Task<int> ProcessInvoiceFromFileAsync(string filePath, string? returnFilePath = null)
    {
        try
        {
            string json;
            try
            {
                json = await File.ReadAllTextAsync(filePath);
            }
            catch (FileNotFoundException)
            {
                OutputHandler.WriteFileNotFoundError(_outputFormat, filePath);
                return ExitCodes.FileNotFoundError;
            }
            catch (Exception ex)
            {
                OutputHandler.WriteError(_outputFormat, ExitCodes.UnexpectedError,
                    $"Failed to read file '{filePath}': {ex.Message}");
                return ExitCodes.UnexpectedError;
            }

            string? returnJson = null;
            if (!string.IsNullOrWhiteSpace(returnFilePath))
            {
                try
                {
                    returnJson = await File.ReadAllTextAsync(returnFilePath);
                }
                catch (FileNotFoundException)
                {
                    OutputHandler.WriteFileNotFoundError(_outputFormat, returnFilePath);
                    return ExitCodes.FileNotFoundError;
                }
            }

            return await ProcessInvoiceAsync(json, returnJson);
        }
        catch (Exception ex)
        {
            OutputHandler.WriteUnexpectedError(_outputFormat, ex);
            return ExitCodes.UnexpectedError;
        }
    }

    /// <summary>
    /// Process invoice from stdin
    /// </summary>
    public async Task<int> ProcessInvoiceFromStdinAsync()
    {
        try
        {
            using var reader = new StreamReader(Console.OpenStandardInput());
            var json = await reader.ReadToEndAsync();
            return await ProcessInvoiceAsync(json);
        }
        catch (Exception ex)
        {
            OutputHandler.WriteUnexpectedError(_outputFormat, ex);
            return ExitCodes.UnexpectedError;
        }
    }

    private async Task<ShamDevs.EFatoraJo.Models.Responses.EInvoiceResponse?> SubmitInvoiceAsync(Invoice invoice)
    {
        try
        {
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, _clientId, _secretKey);

            if (response.IsSuccessfullySubmitted())
            {
                return response;
            }

            if (response.IsAlreadySubmitted())
            {
                // If invoice was already submitted, we still return the response
                // This allows processing of return invoices for already submitted invoices
                // The caller will check if we have a return invoice to process
                return response;
            }

            OutputHandler.WriteApiError(_outputFormat, response);
            return null;
        }
        catch (InvoiceValidationException ex)
        {
            OutputHandler.WriteValidationErrors(_outputFormat,
                "Invoice validation failed",
                ex.ValidationErrors?.ToList() ?? new List<string>());
            return null;
        }
        catch (EInvoiceApiException ex) when (ex.StatusCode == 401)
        {
            OutputHandler.WriteAuthenticationError(_outputFormat);
            return null;
        }
        catch (EInvoiceApiException ex)
        {
            OutputHandler.WriteError(_outputFormat, ExitCodes.ApiError,
                $"API Error (HTTP {ex.StatusCode})",
                new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Message = ex.ResponseContent ?? ex.Message
                    }
                });
            return null;
        }
    }

    private async Task<ShamDevs.EFatoraJo.Models.Responses.EInvoiceResponse?> ProcessReturnInvoiceAsync(
        string returnJson, Invoice originalInvoice)
    {
        try
        {
            // Parse return invoice JSON
            Invoice returnInvoice;
            try
            {
                returnInvoice = InvoiceJsonParser.ParseInvoice(returnJson);
            }
            catch (JsonException ex)
            {
                OutputHandler.WriteJsonParseError(_outputFormat,
                    "Failed to parse return invoice JSON",
                    details: ex.Message);
                return null;
            }

            // Create a SalesReturnInvoice using the parsed return invoice and reference to original
            var salesReturnInvoice = new SalesReturnInvoice(
                invoiceNumber: returnInvoice.InvoiceNumber,
                returnedInvoice: originalInvoice,
                uniqueSerialNumber: returnInvoice.UniqueSerialNumber,
                invoiceDate: returnInvoice.InvoiceDate,
                returnReason: returnInvoice.InvoiceNote ?? "Return from JSON");

            // Submit return invoice
            var response = await EFatoraJoSdk.SendReturnFatoraAsync(salesReturnInvoice, _clientId, _secretKey);

            if (response.IsSuccessfullySubmitted())
            {
                return response;
            }

            if (response.IsAlreadySubmitted())
            {
                OutputHandler.WriteError(_outputFormat, ExitCodes.ApiError,
                    "Return invoice was already submitted");
                return null;
            }

            OutputHandler.WriteApiError(_outputFormat, response);
            return null;
        }
        catch (InvoiceValidationException ex)
        {
            OutputHandler.WriteValidationErrors(_outputFormat,
                "Return invoice validation failed",
                ex.ValidationErrors?.ToList() ?? new List<string>());
            return null;
        }
        catch (EInvoiceApiException ex) when (ex.StatusCode == 401)
        {
            OutputHandler.WriteAuthenticationError(_outputFormat);
            return null;
        }
        catch (EInvoiceApiException ex)
        {
            OutputHandler.WriteError(_outputFormat, ExitCodes.ApiError,
                $"Return Invoice API Error (HTTP {ex.StatusCode})",
                new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Message = $"API Response: {ex.ResponseContent ?? ex.Message}"
                    }
                });
            return null;
        }
        catch (NotSupportedException ex)
        {
            OutputHandler.WriteError(_outputFormat, ExitCodes.ValidationError,
                "Return invoice not supported",
                new List<ErrorDetail>
                {
                    new ErrorDetail { Message = ex.Message }
                });
            return null;
        }
        catch (Exception ex)
        {
            OutputHandler.WriteUnexpectedError(_outputFormat, ex);
            return null;
        }
    }
}
