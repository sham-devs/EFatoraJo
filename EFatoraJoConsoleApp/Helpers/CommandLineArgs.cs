namespace EFatoraJoConsoleApp.Helpers;

/// <summary>
/// Simple command-line argument parser
/// </summary>
public class CommandLineArgs
{
    public string? InvoiceJson { get; set; }
    public string? InvoiceFile { get; set; }
    public string? ReturnJson { get; set; }
    public string? ReturnFile { get; set; }
    public string? OutputFormat { get; set; }
    public string? Sample { get; set; }
    public bool Help { get; set; }
    public bool Version { get; set; }

    public static CommandLineArgs Parse(string[] args)
    {
        var result = new CommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg.ToLower())
            {
                case "--invoice-json":
                    result.InvoiceJson = GetNextArg(args, i++);
                    break;
                case "--invoice-file":
                    result.InvoiceFile = GetNextArg(args, i++);
                    break;
                case "--return-json":
                    result.ReturnJson = GetNextArg(args, i++);
                    break;
                case "--return-file":
                    result.ReturnFile = GetNextArg(args, i++);
                    break;
                case "--output-format":
                    result.OutputFormat = GetNextArg(args, i++);
                    break;
                case "--sample":
                    result.Sample = GetNextArg(args, i++);
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

    private static string? GetNextArg(string[] args, int currentIndex)
    {
        if (currentIndex + 1 < args.Length && !args[currentIndex + 1].StartsWith("--"))
        {
            return args[currentIndex + 1];
        }
        return null;
    }

    public static void ShowHelp()
    {
        Console.WriteLine(@"
EFatoraJo - Jordan E-Invoice CLI

Usage:
  EFatoraJoConsoleApp [options]
  EFatoraJoConsoleApp --invoice-json <json>
  EFatoraJoConsoleApp --invoice-file <path>
  EFatoraJoConsoleApp --sample <type>

Options:
  --invoice-json <json>      Invoice JSON string (use '-' for stdin)
  --invoice-file <path>      Path to invoice JSON file
  --return-json <json>       Return invoice JSON string
  --return-file <path>       Path to return invoice JSON file
  --output-format <format>   Output format: json or text (default: text)
  --sample <type>            Display sample JSON: income, general, special, return
  --help, -h, -?             Show this help message
  --version, -v              Show version information

Examples:
  # Submit invoice from JSON string
  EFatoraJoConsoleApp --invoice-json '{...}'

  # Submit invoice from file
  EFatoraJoConsoleApp --invoice-file invoice.json

  # Submit invoice with return invoice
  EFatoraJoConsoleApp --invoice-file invoice.json --return-file return.json

  # Get JSON output
  EFatoraJoConsoleApp --invoice-file invoice.json --output-format json

  # Show sample JSON
  EFatoraJoConsoleApp --sample general

  # Read from stdin
  cat invoice.json | EFatoraJoConsoleApp --invoice-json -

  # Interactive mode (no arguments)
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
        Console.WriteLine("EFatoraJo Console App v1.0.0");
        Console.WriteLine("Jordan E-Invoice Client SDK");
    }
}
