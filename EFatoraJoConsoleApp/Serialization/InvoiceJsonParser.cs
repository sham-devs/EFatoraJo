using ShamDevs.EFatoraJo.Models;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Serialization;

/// <summary>
/// Parser for Invoice JSON with strict validation
/// </summary>
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
    /// Parse Invoice from JSON string
    /// </summary>
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
