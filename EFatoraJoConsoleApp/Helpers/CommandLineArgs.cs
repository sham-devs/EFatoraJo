namespace EFatoraJoConsoleApp.Helpers;

/// <summary>
/// Simple command-line argument parser
/// </summary>
public class CommandLineArgs
{
    public string? InvoiceFile { get; set; }
    public string? ReturnFile { get; set; }
    public string? ClientId { get; set; }
    public string? SecretKey { get; set; }
    public string? OutputFormat { get; set; }
    public string? Sample { get; set; }
    public bool Help { get; set; }
    public bool Version { get; set; }
    public bool Verbose { get; set; }

    public static CommandLineArgs Parse(string[] args)
    {
        var result = new CommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg.ToLower())
            {
                case "--invoice-file":
                    result.InvoiceFile = GetNextArg(args, ref i);
                    break;
                case "--return-file":
                    result.ReturnFile = GetNextArg(args, ref i);
                    break;
                case "--client-id":
                    result.ClientId = GetNextArg(args, ref i);
                    break;
                case "--secret-key":
                    result.SecretKey = GetNextArg(args, ref i);
                    break;
                case "--output-format":
                    result.OutputFormat = GetNextArg(args, ref i);
                    break;
                case "--sample":
                    result.Sample = GetNextArg(args, ref i);
                    break;
                case "--verbose":
                    result.Verbose = true;
                    break;
                case "--help":
                case "-h":
                case "-?":
                    result.Help = true;
                    break;
                case "--version":
                case "-v":
                    result.Version = true;
                    break;
            }
        }

        return result;
    }

    private static string? GetNextArg(string[] args, ref int currentIndex)
    {
        if (currentIndex + 1 < args.Length && !args[currentIndex + 1].StartsWith("--"))
        {
            currentIndex++;
            return args[currentIndex];
        }
        return null;
    }

    /// <summary>
    /// Validates that only one mode is selected (invoice OR return, not both)
    /// </summary>
    /// <returns>Error message if validation fails, null if valid</returns>
    public string? ValidateCommandMode()
    {
        bool hasInvoice = !string.IsNullOrWhiteSpace(InvoiceFile);
        bool hasReturn = !string.IsNullOrWhiteSpace(ReturnFile);

        // Error: Both provided
        if (hasInvoice && hasReturn)
            return "Cannot combine --invoice-file and --return-file. Use only one.";

        // Error: Neither provided
        if (!hasInvoice && !hasReturn)
            return "Either --invoice-file or --return-file must be provided.";

        return null; // Valid
    }

    /// <summary>
    /// Validates that required credentials are provided
    /// </summary>
    /// <returns>List of missing credential parameter names</returns>
    public List<string> ValidateCredentials()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(ClientId))
            missing.Add("--client-id");

        if (string.IsNullOrWhiteSpace(SecretKey))
            missing.Add("--secret-key");

        return missing;
    }

    public static void ShowHelp()
    {
        Console.WriteLine(@"
EFatoraJo - Jordan E-Invoice CLI

Usage:
  EFatoraJoConsoleApp --invoice-file <path> --client-id <id> --secret-key <key>
  EFatoraJoConsoleApp --return-file <path> --client-id <id> --secret-key <key>
  EFatoraJoConsoleApp --sample <type>

Options:
  --invoice-file <path>      Path to invoice JSON file (for regular invoices)
  --return-file <path>       Path to return invoice JSON file (for returns)
  --client-id <id>           API client ID (REQUIRED for invoice submission)
  --secret-key <key>         API secret key (REQUIRED for invoice submission)
  --output-format <format>   Output format: json or text (default: json)
  --sample <type>            Display sample JSON: income, general, special, return
  --verbose                  Show additional context/header output
  --help, -h, -?             Show this help message
  --version, -v              Show version information

Important Rules:
  - You MUST provide either --invoice-file OR --return-file (not both)
  - You MUST provide both --client-id and --secret-key for invoice submission
  - Interactive mode is available when no arguments are provided

Examples:
  # Submit a sales invoice
  EFatoraJoConsoleApp --invoice-file invoice.json --client-id ""ABC123"" --secret-key ""XYZ789""

  # Submit a return invoice
  EFatoraJoConsoleApp --return-file return.json --client-id ""ABC123"" --secret-key ""XYZ789""

  # Show sample JSON
  EFatoraJoConsoleApp --sample income

  # Interactive mode (no credentials needed)
  EFatoraJoConsoleApp

Exit Codes:
  0 - Success
  1 - Validation error
  2 - API error
  3 - Authentication error
  4 - JSON parse error
  5 - Configuration error
  6 - File not found error
  99 - Unexpected error
");
    }

    public static void ShowVersion()
    {
        Console.WriteLine("EFatoraJo Console App v1.0.1");
        Console.WriteLine("Jordan E-Invoice Client SDK");
    }
}
