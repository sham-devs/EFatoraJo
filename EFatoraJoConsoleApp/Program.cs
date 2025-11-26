using EFatoraJoConsoleApp.Commands;
using EFatoraJoConsoleApp.Helpers;
using EFatoraJoConsoleApp.Models;
using EFatoraJoConsoleApp.Output;
using Microsoft.Extensions.Configuration;
using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Models.Responses;
using System.Text;
using System.Text.Json;

namespace EFatoraJoConsoleApp
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                if (args.Length > 0)
                {
                    return await RunCommandLineModeAsync(args);
                }
                else
                {
                    return await RunInteractiveModeAsync();
                }
            }
            catch (Exception ex)
            {
                var formatter = new ResultFormatter(OutputFormat.Text);
                formatter.Write(CommandResult.ErrorResult(
                    ExitCodes.UnexpectedError,
                    "UnexpectedError",
                    ex.Message));
                return ExitCodes.UnexpectedError;
            }
        }

        static async Task<int> RunCommandLineModeAsync(string[] args)
        {
            var cmdArgs = CommandLineArgs.Parse(args);
            var format = ResolveOutputFormat(cmdArgs.OutputFormat);
            var formatter = new ResultFormatter(format);

            // Handle --help
            if (cmdArgs.Help)
            {
                CommandLineArgs.ShowHelp();
                return ExitCodes.Success;
            }

            // Handle --version
            if (cmdArgs.Version)
            {
                CommandLineArgs.ShowVersion();
                return ExitCodes.Success;
            }

            // Handle --sample
            if (!string.IsNullOrEmpty(cmdArgs.Sample))
            {
                ShowSample(cmdArgs.Sample);
                return ExitCodes.Success;
            }

            // Validate command mode (invoice XOR return)
            string? modeError = cmdArgs.ValidateCommandMode();
            if (modeError != null)
            {
                var modeErrorResult = CommandResult.ErrorResult(
                    ExitCodes.ConfigurationError,
                    "CommandModeError",
                    modeError,
                    new List<string> { modeError });
                formatter.Write(modeErrorResult);
                return modeErrorResult.ExitCode;
            }

            // Validate credentials are provided
            var missingCreds = cmdArgs.ValidateCredentials();
            if (missingCreds.Count > 0)
            {
                var missingCredsResult = CommandResult.ErrorResult(
                    ExitCodes.ConfigurationError,
                    "MissingCredentials",
                    "Required credentials not provided",
                    missingCreds);
                formatter.Write(missingCredsResult);
                return missingCredsResult.ExitCode;
            }

            // Create command handler with credentials from command line
            var handler = new InvoiceCommandHandler(cmdArgs.ClientId!, cmdArgs.SecretKey!);
            CommandResult commandResult;

            if (cmdArgs.Verbose)
            {
                formatter.WriteIntro("Command mode");
            }

            // Route to appropriate handler based on command type
            if (!string.IsNullOrWhiteSpace(cmdArgs.InvoiceFile))
            {
                commandResult = await handler.ProcessInvoiceCommand(cmdArgs.InvoiceFile);
            }
            else if (!string.IsNullOrWhiteSpace(cmdArgs.ReturnFile))
            {
                commandResult = await handler.ProcessReturnInvoiceCommand(cmdArgs.ReturnFile);
            }
            else
            {
                commandResult = CommandResult.ErrorResult(
                    ExitCodes.ConfigurationError,
                    "CommandModeError",
                    "Either --invoice-file or --return-file must be provided.");
            }

            formatter.Write(commandResult);
            return commandResult.ExitCode;
        }

        static async Task<int> RunInteractiveModeAsync()
        {
            var formatter = new ResultFormatter(OutputFormat.Text);
            Console.WriteLine("EFatoraJo Interactive Mode");
            Console.WriteLine(new string('-', 40));

            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var clientId = config["EFatora:ClientId"];
            var secretKey = config["EFatora:SecretKey"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(secretKey))
            {
                var result = CommandResult.ErrorResult(
                    ExitCodes.ConfigurationError,
                    "ConfigurationError",
                    "Missing configuration in User Secrets",
                    new List<string>
                    {
                        "EFatora:ClientId",
                        "EFatora:SecretKey"
                    });
                formatter.Write(result);
                return result.ExitCode;
            }

            Console.WriteLine("Credentials found. Press Ctrl+C to exit at any time.");

            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine("Select Invoice Type:");
                    Console.WriteLine("  1 - Income Invoice");
                    Console.WriteLine("  2 - General Sales Invoice");
                    Console.WriteLine("  3 - Special Sales Invoice");
                    Console.WriteLine("  0 - Exit");
                    Console.Write("\nChoice: ");

                    var choice = Console.ReadLine();

                    if (choice == "0")
                    {
                        break;
                    }

                    InvoiceType invoiceType = choice switch
                    {
                        "1" => InvoiceType.Income,
                        "2" => InvoiceType.GeneralSales,
                        "3" => InvoiceType.SpecialSales,
                        _ => throw new InvalidOperationException("Invalid choice")
                    };

                    var invoice = GenerateRandomInvoice(invoiceType, config);

                    Console.WriteLine("\nGenerated Invoice:");
                    Console.WriteLine($"  Type: {invoiceType}");
                    Console.WriteLine($"  Number: {invoice.InvoiceNumber}");
                    Console.WriteLine($"  Date: {invoice.InvoiceDate}");
                    Console.WriteLine($"  Customer: {invoice.Customer.Name}");
                    Console.WriteLine($"  Total: {invoice.InvoiceTotals.TotalInvoiceAmount:F2} JOD");

                    Console.Write("\nSubmit invoice now? [Y/n]: ");
                    var submitChoice = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(submitChoice) || submitChoice.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
                        var result = BuildCommandResultFromResponse(response, "Invoice submitted successfully");
                        formatter.Write(result);
                    }

                    Console.Write("\nCreate return invoice? [y/N]: ");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        var returnInvoice = new SalesReturnInvoice(
                            invoiceNumber: $"RET-{DateTime.Now:yyyyMMdd-HHmmss}",
                            returnedInvoice: invoice,
                            uniqueSerialNumber: Guid.NewGuid().ToString(),
                            invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
                            returnReason: "Customer return - Interactive mode"
                        );

                        Console.Write("\nSubmit return invoice now? [Y/n]: ");
                        var submitReturn = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(submitReturn) || submitReturn.Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            var returnResponse = await EFatoraJoSdk.SendReturnFatoraAsync(returnInvoice, clientId, secretKey);
                            var result = BuildCommandResultFromResponse(returnResponse, "Return invoice submitted successfully");
                            formatter.Write(result);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                }
                catch (InvoiceValidationException ex)
                {
                    formatter.Write(CommandResult.ErrorResult(
                        ExitCodes.ValidationError,
                        "InvoiceValidationException",
                        ex.Message,
                        ex.ValidationErrors?.ToList() ?? new List<string>()));
                }
                catch (Exception ex)
                {
                    formatter.Write(CommandResult.ErrorResult(
                        ExitCodes.UnexpectedError,
                        "UnexpectedError",
                        ex.Message));
                }
            }

            Console.WriteLine("\nGoodbye!");
            return ExitCodes.Success;
        }

        static CommandResult BuildCommandResultFromResponse(EInvoiceResponse response, string successMessage)
        {
            if (response.IsSuccessfullySubmitted())
            {
                return CommandResult.SuccessResult(response, successMessage);
            }

            if (response.IsAlreadySubmitted())
            {
                return CommandResult.SuccessResult(response, "Invoice was already submitted", alreadySubmitted: true);
            }

            return InvoiceCommandHandler.CreateApiErrorResult(response);
        }

        static void ShowSample(string sampleType)
        {
            try
            {
                var sampleJson = EFatoraJoConsoleApp.Samples.SampleProvider.GetSampleJson(sampleType);
                Console.WriteLine(sampleJson);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Unknown sample type: {sampleType}");
                Console.WriteLine("Available samples: income, general, special, return");
            }
        }

        static Invoice GenerateRandomInvoice(InvoiceType invoiceType, IConfiguration config)
        {
            var random = new Random();
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var invoiceNumber = $"INV-{timestamp}";

            var customerNames = new[] { "Ahmad Hassan", "Fatima Ali", "Mohammed Ibrahim", "Sara Khalil", "Omar Mustafa" };
            var customerName = customerNames[random.Next(customerNames.Length)];

            var paymentTypes = GetValidPaymentTypesForInvoiceType(invoiceType);
            var paymentType = paymentTypes[random.Next(paymentTypes.Length)];

            var customer = new Customer(customerName);

            var itemNames = new[] { "Product A", "Service B", "Item C", "Consulting D", "Equipment E" };
            var itemCount = random.Next(1, 4);
            var invoiceDetails = new List<InvoiceDetail>();
            decimal totalBeforeVAT = 0;
            decimal totalVAT = 0;

            var taxCategory = invoiceType == InvoiceType.Income ? TaxCategoryCode.Z : TaxCategoryCode.S;

            for (int i = 0; i < itemCount; i++)
            {
                var quantity = random.Next(1, 10);
                var price = Math.Round((decimal)(random.NextDouble() * 100 + 10), 2);
                var lineTotal = quantity * price;

                var vatPercent = invoiceType == InvoiceType.Income ? 0.0 : 16.0;
                var vatAmount = Math.Round(lineTotal * (decimal)(vatPercent / 100), 2);

                var detail = new InvoiceDetail(
                    id: $"LINE-{i + 1}",
                    taxCategory: taxCategory,
                    description: itemNames[random.Next(itemNames.Length)]
                )
                {
                    Quantity = quantity,
                    UnitPriceBeforeTax = price,
                    TotalBeforeTax = lineTotal,
                    TaxAmount = vatAmount,
                    TotalIncludingTax = lineTotal + vatAmount
                };

                invoiceDetails.Add(detail);

                totalBeforeVAT += lineTotal;
                totalVAT += vatAmount;
            }

            var invoiceTotals = new InvoiceTotals
            {
                TotalVATAmount = totalVAT,
                TotalSpecialTaxAmount = 0,
                TotalBeforeDiscount = totalBeforeVAT,
                TotalInvoiceAmount = totalBeforeVAT + totalVAT,
                TotalDiscountAmount = 0,
                FinalPayableAmount = totalBeforeVAT + totalVAT
            };

            var supplier = new Supplier(
                taxVATNumber: config["EFatora:Supplier:TaxNumber"] ?? "0000000000",
                incomeSourceSequence: config["EFatora:Supplier:ActivityCode"] ?? "62010",
                registeredSupplierName: config["EFatora:Supplier:Name"] ?? "Default Supplier"
            );

            return new Invoice(
                invoiceNumber: invoiceNumber,
                uniqueSerialNumber: Guid.NewGuid().ToString(),
                invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
                paymentType: paymentType,
                supplier: supplier,
                customer: customer,
                invoiceTotals: invoiceTotals,
                invoiceDetails: invoiceDetails,
                type: invoiceType
            )
            {
                InvoiceNote = $"Test invoice generated at {DateTime.Now}"
            };
        }

        static InvoicePaymentTypeCode[] GetValidPaymentTypesForInvoiceType(InvoiceType invoiceType)
        {
            return invoiceType switch
            {
                InvoiceType.Income => [InvoicePaymentTypeCode.LocalIncomeCash, InvoicePaymentTypeCode.LocalIncomeCredit],
                InvoiceType.GeneralSales => [InvoicePaymentTypeCode.LocalGeneralSalesCash, InvoicePaymentTypeCode.LocalGeneralSalesCredit],
                InvoiceType.SpecialSales => [InvoicePaymentTypeCode.LocalSpecialSalesCash, InvoicePaymentTypeCode.LocalSpecialSalesCredit],
                _ => [InvoicePaymentTypeCode.LocalGeneralSalesCash]
            };
        }

        static OutputFormat ResolveOutputFormat(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                value.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                return OutputFormat.Text;
            }

            return OutputFormat.Json;
        }
    }
}



