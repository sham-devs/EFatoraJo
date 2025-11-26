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
using static EFatoraJoConsoleApp.Output.OutputHandler;

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
                OutputHandler.WriteUnexpectedError(OutputFormat.Text, ex);
                return ExitCodes.UnexpectedError;
            }
        }

        static async Task<int> RunCommandLineModeAsync(string[] args)
        {
            var cmdArgs = CommandLineArgs.Parse(args);

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
                WriteJsonError("CommandModeError", modeError);
                return ExitCodes.ConfigurationError;
            }

            // Validate credentials are provided
            var missingCreds = cmdArgs.ValidateCredentials();
            if (missingCreds.Count > 0)
            {
                WriteJsonError("MissingCredentials",
                    "Required credentials not provided",
                    missingCreds);
                return ExitCodes.ConfigurationError;
            }

            // Create command handler with credentials from command line
            var handler = new InvoiceCommandHandler(cmdArgs.ClientId!, cmdArgs.SecretKey!);

            // Route to appropriate handler based on command type
            if (!string.IsNullOrWhiteSpace(cmdArgs.InvoiceFile))
            {
                return await handler.ProcessInvoiceCommand(cmdArgs.InvoiceFile);
            }
            else if (!string.IsNullOrWhiteSpace(cmdArgs.ReturnFile))
            {
                return await handler.ProcessReturnInvoiceCommand(cmdArgs.ReturnFile);
            }

            // Should never reach here due to ValidateCommandMode
            WriteJsonError("InternalError", "Unexpected code path");
            return ExitCodes.UnexpectedError;
        }

        static void WriteJsonError(string errorType, string message, List<string>? errors = null)
        {
            var output = new
            {
                success = false,
                errorType = errorType,
                message = message,
                errors = errors ?? new List<string>()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            Console.WriteLine(JsonSerializer.Serialize(output, options));
        }

        static async Task<int> RunInteractiveModeAsync()
        {
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║   EFatoraJo - Jordan E-Invoice Client      ║");
            Console.WriteLine("║   Console Test Application                 ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine();

            // Load configuration
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var clientId = config["EFatora:ClientId"];
            var secretKey = config["EFatora:SecretKey"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine("❌ Error: Missing configuration in User Secrets");
                Console.WriteLine("Please configure EFatora:ClientId and EFatora:SecretKey");
                Console.WriteLine("\nTo set user secrets, run:");
                Console.WriteLine("  dotnet user-secrets set \"EFatora:ClientId\" \"your-client-id\"");
                Console.WriteLine("  dotnet user-secrets set \"EFatora:SecretKey\" \"your-secret-key\"");
                return ExitCodes.ConfigurationError;
            }

            Console.WriteLine($"✓ Client ID: {clientId}");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    // Select invoice type
                    Console.WriteLine("\n═══════════════════════════════════════════");
                    Console.WriteLine("Select Invoice Type:");
                    Console.WriteLine("  1 - Income Invoice (فاتورة دخل)");
                    Console.WriteLine("  2 - General Sales Invoice (فاتورة مبيعات عامة)");
                    Console.WriteLine("  3 - Special Sales Invoice (فاتورة مبيعات خاصة)");
                    Console.WriteLine("  0 - Exit");
                    Console.Write("\nChoice: ");

                    var choice = Console.ReadLine();

                    if (choice == "0") break;

                    InvoiceType invoiceType = choice switch
                    {
                        "1" => InvoiceType.Income,
                        "2" => InvoiceType.GeneralSales,
                        "3" => InvoiceType.SpecialSales,
                        _ => throw new InvalidOperationException("Invalid choice")
                    };

                    // Generate random invoice
                    var invoice = GenerateRandomInvoice(invoiceType, config);

                    Console.WriteLine("\n📄 Generated Invoice:");
                    Console.WriteLine($"   Type: {invoiceType}");
                    Console.WriteLine($"   Number: {invoice.InvoiceNumber}");
                    Console.WriteLine($"   Date: {invoice.InvoiceDate}");
                    Console.WriteLine($"   Customer: {invoice.Customer.Name}");
                    Console.WriteLine($"   Total: {invoice.InvoiceTotals.TotalInvoiceAmount:F2} JOD");

                    // Submit invoice
                    Console.WriteLine("\n📤 Submitting invoice...");
                    var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);

                    if (response.IsSuccessfullySubmitted())
                    {
                        Console.WriteLine("\n✅ Invoice submitted successfully!");
                        Console.WriteLine($"   Invoice Number: {response.InvoiceNumber}");
                        Console.WriteLine($"   QR Code: {response.Qr}");
                    }
                    else
                    {
                        HandleFailedResponse(response);
                    }

                    // Ask about return invoice
                    Console.Write("\n🔄 Create return invoice? (y/n): ");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        var returnInvoice = new SalesReturnInvoice(
                            invoiceNumber: $"RET-{DateTime.Now:yyyyMMdd-HHmmss}",
                            returnedInvoice: invoice,
                            uniqueSerialNumber: Guid.NewGuid().ToString(),
                            invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
                            returnReason: "Customer return - Interactive mode"
                        );

                        Console.WriteLine("\n📤 Submitting return invoice...");
                        var returnResponse = await EFatoraJoSdk.SendReturnFatoraAsync(returnInvoice, clientId, secretKey);

                        if (returnResponse.IsSuccessfullySubmitted())
                        {
                            Console.WriteLine("\n✅ Return invoice submitted successfully!");
                            Console.WriteLine($"   Return Invoice Number: {returnResponse.InvoiceNumber}");
                        }
                        else
                        {
                            HandleFailedResponse(returnResponse);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("❌ Invalid choice. Please try again.");
                }
                catch (InvoiceValidationException ex)
                {
                    Console.WriteLine($"\n❌ Validation Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Error: {ex.Message}");
                }
            }

            Console.WriteLine("\n👋 Goodbye!");
            return ExitCodes.Success;
        }

        static void ShowSample(string sampleType)
        {
            var samples = new Dictionary<string, string>
            {
                ["income"] = @"{
  ""invoiceNumber"": ""INV-2024-001"",
  ""uniqueSerialNumber"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""invoiceDate"": ""2024-01-15"",
  ""invoiceType"": ""Income"",
  ""paymentType"": ""Cash"",
  ""customer"": {
    ""name"": ""Ahmad Hassan"",
    ""taxNumber"": ""1234567890"",
    ""mobileNumber"": ""962791234567""
  },
  ""invoiceLines"": [
    {
      ""itemName"": ""Consulting Services"",
      ""itemQuantity"": 10,
      ""itemPrice"": 100.00,
      ""totalAmount"": 1000.00
    }
  ],
  ""totalInvoiceAmount"": 1000.00,
  ""invoiceNote"": ""Professional consulting services""
}",
                ["general"] = @"{
  ""invoiceNumber"": ""GEN-2024-001"",
  ""uniqueSerialNumber"": ""550e8400-e29b-41d4-a716-446655440001"",
  ""invoiceDate"": ""2024-01-15"",
  ""invoiceType"": ""GeneralSales"",
  ""paymentType"": ""Cash"",
  ""customer"": {
    ""name"": ""Retail Customer"",
    ""mobileNumber"": ""962791234567""
  },
  ""invoiceLines"": [
    {
      ""itemName"": ""Product A"",
      ""itemQuantity"": 5,
      ""itemPrice"": 20.00,
      ""taxPercent"": 16.0,
      ""totalAmount"": 100.00
    }
  ],
  ""totalInvoiceAmount"": 116.00,
  ""invoiceNote"": ""General sales invoice""
}",
                ["special"] = @"{
  ""invoiceNumber"": ""SPC-2024-001"",
  ""uniqueSerialNumber"": ""550e8400-e29b-41d4-a716-446655440002"",
  ""invoiceDate"": ""2024-01-15"",
  ""invoiceType"": ""SpecialSales"",
  ""paymentType"": ""Cash"",
  ""customer"": {
    ""name"": ""Special Customer"",
    ""taxNumber"": ""9876543210"",
    ""mobileNumber"": ""962791234567""
  },
  ""invoiceLines"": [
    {
      ""itemName"": ""Special Item"",
      ""itemQuantity"": 3,
      ""itemPrice"": 150.00,
      ""taxPercent"": 16.0,
      ""totalAmount"": 450.00
    }
  ],
  ""totalInvoiceAmount"": 522.00,
  ""invoiceNote"": ""Special sales invoice""
}",
                ["return"] = @"{
  ""invoiceNumber"": ""RET-2024-001"",
  ""uniqueSerialNumber"": ""550e8400-e29b-41d4-a716-446655440003"",
  ""invoiceDate"": ""2024-01-15"",
  ""invoiceType"": ""Income"",
  ""paymentType"": ""Cash"",
  ""customer"": {
    ""name"": ""Return Customer"",
    ""mobileNumber"": ""962791234567""
  },
  ""invoiceLines"": [
    {
      ""itemName"": ""Returned Item"",
      ""itemQuantity"": 2,
      ""itemPrice"": 50.00,
      ""totalAmount"": 100.00
    }
  ],
  ""totalInvoiceAmount"": 100.00,
  ""invoiceNote"": ""Product return""
}"
            };

            if (samples.TryGetValue(sampleType.ToLower(), out var sample))
            {
                Console.WriteLine(sample);
            }
            else
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

        static void HandleFailedResponse(EInvoiceResponse response)
        {
            Console.WriteLine("\n❌ Invoice submission failed!");

            if (response.Results?.Errors != null && response.Results.Errors.Count > 0)
            {
                Console.WriteLine("\nErrors:");
                foreach (var error in response.Results.Errors)
                {
                    Console.WriteLine($"  - {error.Message}");
                }
            }

            if (response.Results?.Warnings != null && response.Results.Warnings.Count > 0)
            {
                Console.WriteLine("\nWarnings:");
                foreach (var warning in response.Results.Warnings)
                {
                    Console.WriteLine($"  - {warning.Message}");
                }
            }
        }
    }
}
