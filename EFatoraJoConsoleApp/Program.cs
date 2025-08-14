// Program.cs
using Microsoft.Extensions.Configuration;
using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Models.Responses;
using ShamDevs.EFatoraJo.Utilities;
using System.Text;

namespace EFatoraJoConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("EFatoraJo Invoice Generator");
            Console.WriteLine("===========================");

            // ------------------------------------------------------------------
            // Read ALL required secrets
            // ------------------------------------------------------------------
            var cfg = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            string clientId = cfg["ClientId"] ?? string.Empty;
            string secretKey = cfg["SecretKey"] ?? string.Empty;

            var supplierCfg = cfg.GetSection("Supplier");
            string taxVATNumber = supplierCfg["TaxVATNumber"] ?? string.Empty;
            string incomeSourceSequence = supplierCfg["IncomeSourceSequence"] ?? string.Empty;
            string registeredSupplierName = supplierCfg["RegisteredSupplierName"] ?? string.Empty;

            bool missing = false;
            void CheckSecret(string value, string name)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: Missing secret \"{name}\"");
                    Console.ResetColor();
                    missing = true;
                }
            }

            CheckSecret(clientId, nameof(clientId));
            CheckSecret(secretKey, nameof(secretKey));
            CheckSecret(taxVATNumber, "Supplier:TaxVATNumber");
            CheckSecret(incomeSourceSequence, "Supplier:IncomeSourceSequence");
            CheckSecret(registeredSupplierName, "Supplier:RegisteredSupplierName");

            if (missing)
            {
                Console.WriteLine("\nSet the missing secrets with:");
                Console.WriteLine("  dotnet user-secrets set <key> \"<value>\"");
                return;
            }

            var supplierInfo = new Supplier(
                taxVATNumber ?? string.Empty,
                incomeSourceSequence ?? string.Empty,
                registeredSupplierName ?? string.Empty);

            try
            {
                // ------------------------------------------------------------------
                // Invoice Type selection
                // ------------------------------------------------------------------
                Console.WriteLine("\nAvailable Invoice Types:");
                Console.WriteLine("1: General Sales (with return capability)");
                Console.WriteLine("2: Special Sales (with return capability)");
                Console.WriteLine("3: Income (no returns)");

                int invoiceTypeChoice;
                while (true)
                {
                    Console.Write("Select invoice type (1-3, blank=1): ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        invoiceTypeChoice = 1;
                        break;
                    }

                    if (int.TryParse(input, out invoiceTypeChoice) && invoiceTypeChoice >= 1 && invoiceTypeChoice <= 3)
                    {
                        break;
                    }
                    Console.WriteLine("Invalid input. Please enter 1, 2, or 3.");
                }

                var invoiceType = invoiceTypeChoice switch
                {
                    1 => InvoiceType.GeneralSales,
                    2 => InvoiceType.SpecialSales,
                    3 => InvoiceType.Income,
                    _ => InvoiceType.GeneralSales
                };

                // ------------------------------------------------------------------
                // Invoice Payment Type Code selection
                // ------------------------------------------------------------------
                Console.WriteLine("\nAvailable Invoice Payment Type Codes:");
                var validPaymentTypes = GetValidPaymentTypesForInvoiceType(invoiceType).ToList();
                DisplayPaymentTypes(validPaymentTypes);

                InvoicePaymentTypeCode paymentType;
                while (true)
                {
                    Console.Write($"Enter Invoice Payment Type Code (1-{validPaymentTypes.Count}, blank={GetDefaultPaymentTypeForInvoiceType(invoiceType)}): ");
                    string? input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        paymentType = GetDefaultPaymentTypeForInvoiceType(invoiceType);
                        break;
                    }

                    if (int.TryParse(input, out int selectedIndex) &&
                        selectedIndex >= 1 &&
                        selectedIndex <= validPaymentTypes.Count)
                    {
                        paymentType = validPaymentTypes[selectedIndex - 1];
                        break;
                    }

                    Console.WriteLine($"Invalid input. Please enter a number between 1 and {validPaymentTypes.Count}.");
                }

                // ------------------------------------------------------------------
                // Currency selection
                // ------------------------------------------------------------------
                Console.WriteLine("\nAvailable Currencies:");
                foreach (var c in Enum.GetValues<CurrencyCode>())
                    Console.WriteLine($"{(int)c}: {c} ({c.GetStringValue()})");

                CurrencyCode currency = GetEnumInput(
                    "Enter Currency Code (blank = JOD): ",
                    CurrencyCode.JOD);

                // ------------------------------------------------------------------
                // Generate and submit main invoice
                // ------------------------------------------------------------------
                Console.WriteLine("\nGenerating and submitting invoice...");
                var invoice = RandomInvoiceGenerator.GenerateRandomInvoice(
                    supplierInfo: supplierInfo,
                    invoicePaymentType: paymentType,
                    currency: currency,
                    invoiceTpe: invoiceType);

                var invoiceResponse = await EFatoraJoSdk.SendFatoraAsync(
                    invoice,
                    clientId ?? string.Empty,
                    secretKey ?? string.Empty);

                // In the Main method where we handle the invoice response:
                if (invoiceResponse.IsSuccessfullySubmitted())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nInvoice {invoiceResponse.InvoiceNumber} submitted successfully!");
                    Console.WriteLine($"QR Code: {invoiceResponse.Qr}");
                    Console.ResetColor();

                    // ------------------------------------------------------------------
                    // Generate and submit return invoice for ALL supported types
                    // ------------------------------------------------------------------
                    Console.WriteLine("\nGenerating and submitting return invoice...");

                    try
                    {
                        var returnInvoice = RandomInvoiceGenerator.GenerateRandomSalesReturnInvoice(
                            originalInvoice: invoice,
                            currency: currency);

                        var returnResponse = await EFatoraJoSdk.SendReturnFatoraAsync(
                            returnInvoice,
                            clientId ?? string.Empty,
                            secretKey ?? string.Empty);

                        if (returnResponse.IsSuccessfullySubmitted())
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nReturn invoice submitted successfully!");
                            Console.WriteLine($"Return Reference: {returnResponse.InvoiceNumber}");
                            Console.WriteLine($"For Original Invoice: {returnInvoice.ReturnedInvoice?.InvoiceNumber ?? "N/A"}");
                            Console.ResetColor();
                        }
                        else
                        {
                            HandleFailedResponse(returnResponse, "return invoice");
                        }
                    }
                    catch (NotSupportedException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\nReturn invoice not supported: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            catch (InvoiceValidationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvoice validation failed:");
                foreach (var error in ex.ValidationErrors ?? [])
                    Console.WriteLine($"- {error}");
                Console.ResetColor();
            }
            catch (EInvoiceApiException ex) when (ex.StatusCode == 401)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAuthentication failed – please check your secrets.");
                Console.ResetColor();
            }
            catch (EInvoiceApiException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAPI Error ({ex.StatusCode}):\n{ex.ResponseContent ?? string.Empty}");
                Console.ResetColor();
            }
            catch (EInvoiceException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nE-Invoice Error: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nUnexpected error: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
<<<<<<< HEAD
            //Console.ReadKey();
=======
            Console.ReadKey();
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
        }

        // ------------------------------------------------------------------
        // Helper methods
        // ------------------------------------------------------------------
        private static TEnum GetEnumInput<TEnum>(string prompt, TEnum defaultValue, IEnumerable<TEnum>? validValues = null)
            where TEnum : struct, Enum
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return defaultValue;

                if (Enum.TryParse<TEnum>(input, out var result))
                {
                    if (Enum.IsDefined(result) && (validValues == null || validValues.Contains(result)))
                        return result;
                }

                if (int.TryParse(input, out int intVal) && Enum.IsDefined(typeof(TEnum), intVal))
                {
                    var res = (TEnum)(object)intVal;
                    if (validValues == null || validValues.Contains(res))
                        return res;
                }

                Console.WriteLine($"Invalid input. Please enter a valid {typeof(TEnum).Name} value or code.");
            }
        }

        private static void HandleFailedResponse(EInvoiceResponse? response, string invoiceType)
        {
            if (response == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nReceived null response for {invoiceType}");
                Console.ResetColor();
                return;
            }

            if (response.IsAlreadySubmitted())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n{invoiceType} was already submitted");
                if (!string.IsNullOrEmpty(response.Qr))
                    Console.WriteLine($"This {invoiceType} has a valid QR code: {response.Qr}");
                Console.ResetColor();
            }
            else if (response.HasErrors())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n{invoiceType} submission failed with errors:");
                Console.WriteLine(response.GetFormattedErrors());

                if (response.HasWarnings())
                {
                    Console.WriteLine("\nAdditional warnings:");
                    Console.WriteLine(response.GetFormattedWarnings());
                }
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nReceived unknown response status for {invoiceType}");
                Console.ResetColor();
            }
        }

        private static InvoicePaymentTypeCode[] GetValidPaymentTypesForInvoiceType(InvoiceType invoiceType)
        {
            return invoiceType switch
            {
                InvoiceType.Income =>
                [
                    InvoicePaymentTypeCode.LocalIncomeCash,       // 010
                    InvoicePaymentTypeCode.LocalIncomeCredit,     // 020
                    InvoicePaymentTypeCode.ExportIncomeCash,      // 011
                    InvoicePaymentTypeCode.ExportIncomeCredit      // 021
                ],
                InvoiceType.GeneralSales =>
                [
                    InvoicePaymentTypeCode.LocalGeneralSalesCash,     // 012
                    InvoicePaymentTypeCode.LocalGeneralSalesCredit,    // 022
                    InvoicePaymentTypeCode.ExportGeneralSalesCash,    // 013
                    InvoicePaymentTypeCode.ExportGeneralSalesCredit   // 023
                ],
                InvoiceType.SpecialSales =>
                [
                    InvoicePaymentTypeCode.LocalSpecialSalesCash,     // 014
                    InvoicePaymentTypeCode.LocalSpecialSalesCredit,    // 024
                    InvoicePaymentTypeCode.ExportSpecialSalesCash,     // 015
                    InvoicePaymentTypeCode.ExportSpecialSalesCredit    // 025
                ],
                _ => Enum.GetValues<InvoicePaymentTypeCode>()
            };
        }

        private static InvoicePaymentTypeCode GetDefaultPaymentTypeForInvoiceType(InvoiceType invoiceType)
        {
            return invoiceType switch
            {
                InvoiceType.Income => InvoicePaymentTypeCode.LocalIncomeCredit,
                InvoiceType.GeneralSales => InvoicePaymentTypeCode.LocalGeneralSalesCredit,
                InvoiceType.SpecialSales => InvoicePaymentTypeCode.LocalSpecialSalesCredit,
                _ => InvoicePaymentTypeCode.LocalGeneralSalesCredit
            };
        }

        private static void DisplayPaymentTypes(IEnumerable<InvoicePaymentTypeCode> paymentTypes)
        {
            int index = 1; // Changed from 0 to 1
            foreach (var t in paymentTypes)
            {
                Console.WriteLine($"{index}: {t} ({t.GetStringValue()})");
                index++;
            }
        }
    }
}