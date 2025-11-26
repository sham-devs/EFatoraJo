using EFatoraJoConsoleApp.Models;
using System.Text;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Output;

public enum OutputFormat
{
    Json,
    Text
}

/// <summary>
/// Single formatter responsible for rendering CommandResult and setting exit codes.
/// </summary>
public class ResultFormatter
{
    private readonly OutputFormat _format;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ResultFormatter(OutputFormat format)
    {
        _format = format;
    }

    public CommandResult Write(CommandResult result)
    {
        Environment.ExitCode = result.ExitCode;

        if (_format == OutputFormat.Json)
        {
            WriteJson(result);
        }
        else
        {
            WriteText(result);
        }

        return result;
    }

    private void WriteJson(CommandResult result)
    {
        var payload = new
        {
            success = result.Success,
            message = result.Message,
            errorType = result.ErrorType,
            errors = result.Errors,
            exitCode = result.ExitCode,
            alreadySubmitted = result.AlreadySubmitted ? true : null,
            result = result.Result
        };

        Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private void WriteText(CommandResult result)
    {
        var builder = new StringBuilder();

        if (result.Success)
        {
            builder.AppendLine($"SUCCESS: {result.Message ?? "Operation completed"}");
            if (result.AlreadySubmitted)
            {
                builder.AppendLine("Note: Invoice was already submitted previously.");
            }
        }
        else
        {
            builder.AppendLine($"ERROR ({result.ExitCode} - {result.ErrorType ?? "Unknown"}): {result.Message}");
            if (result.Errors.Any())
            {
                builder.AppendLine("Details:");
                foreach (var error in result.Errors)
                {
                    builder.AppendLine($"- {error}");
                }
            }
        }

        // Append minimal result info for text mode if present
        if (result.Result is ShamDevs.EFatoraJo.Models.Responses.EInvoiceResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.InvoiceNumber))
            {
                builder.AppendLine($"Invoice Number: {response.InvoiceNumber}");
            }
            if (!string.IsNullOrWhiteSpace(response.InvoiceUuid))
            {
                builder.AppendLine($"Invoice UUID: {response.InvoiceUuid}");
            }
            if (!string.IsNullOrWhiteSpace(response.Qr))
            {
                builder.AppendLine("QR: [hidden for security]");
            }
        }

        Console.WriteLine(builder.ToString().TrimEnd());
    }

    public void WriteIntro(string? context = null)
    {
        if (_format != OutputFormat.Text)
        {
            return; // Avoid breaking JSON output
        }

        var lines = new List<string>
        {
            "EFatoraJo Console (net8.0) - Jordan E-Invoice Client",
            context ?? "Command mode",
            $"Format: {_format}"
        };

        Console.WriteLine(string.Join(Environment.NewLine, lines));
        Console.WriteLine(new string('-', 60));
    }
}
