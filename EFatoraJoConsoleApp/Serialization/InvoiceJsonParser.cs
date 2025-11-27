using ShamDevs.EFatoraJo.Models;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Serialization;

/// <summary>
/// Parser for regular invoice JSON with strict validation.
/// This parser is isolated from <see cref="ReturnInvoiceJsonParser"/> and handles only regular invoice files.
/// </summary>
/// <remarks>
/// <para>This parser validates and rejects return invoice files to ensure path isolation.</para>
/// <para>Key validation rules:</para>
/// <list type="bullet">
///   <item><description>Rejects files containing 'returnReason' or 'returnedInvoice' properties</description></item>
///   <item><description>Requires standard invoice fields: invoiceNumber, uniqueSerialNumber, invoiceDate, paymentType</description></item>
///   <item><description>All monetary amounts remain positive (no sign conversion)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ReturnInvoiceJsonParser"/>
public static class InvoiceJsonParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new InvoiceJsonConverter() }
    };

    /// <summary>
    /// Parse a regular invoice from JSON string with strict validation.
    /// </summary>
    /// <param name="json">JSON string containing invoice data</param>
    /// <returns>Validated <see cref="Invoice"/> with all positive amounts</returns>
    /// <exception cref="JsonException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>JSON is empty or malformed</description></item>
    ///   <item><description>Required fields are missing</description></item>
    ///   <item><description>A return invoice file is used (contains returnReason or returnedInvoice)</description></item>
    /// </list>
    /// </exception>
    public static Invoice ParseInvoice(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException("JSON input cannot be empty");
        }

        try
        {
            var invoice = JsonSerializer.Deserialize<Invoice>(json, Options);
            if (invoice == null)
            {
                throw new JsonException("Failed to deserialize invoice - result was null");
            }
            return invoice;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Invalid JSON format: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parse Invoice from file
    /// </summary>
    public static async Task<Invoice> ParseInvoiceFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Invoice file not found: {filePath}");
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to read file '{filePath}': {ex.Message}", ex);
        }

        return ParseInvoice(json);
    }

    /// <summary>
    /// Parse Invoice from stdin
    /// </summary>
    public static async Task<Invoice> ParseInvoiceFromStdinAsync()
    {
        using var reader = new StreamReader(Console.OpenStandardInput());
        var json = await reader.ReadToEndAsync();
        return ParseInvoice(json);
    }
}
