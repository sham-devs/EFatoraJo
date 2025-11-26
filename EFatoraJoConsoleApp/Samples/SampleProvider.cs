using EFatoraJoConsoleApp.Serialization;
using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Samples;

/// <summary>
/// Generates sample payloads using SDK models and the same serializer options used by the CLI.
/// </summary>
public static class SampleProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string GetSampleJson(string sampleType)
    {
        return sampleType.ToLowerInvariant() switch
        {
            "income" => SerializeInvoice(CreateIncomeInvoice()),
            "general" => SerializeInvoice(CreateGeneralInvoice()),
            "special" => SerializeInvoice(CreateSpecialInvoice()),
            "return" => SerializeReturnInvoice(),
            _ => throw new ArgumentException($"Unknown sample type: {sampleType}")
        };
    }

    private static string SerializeInvoice(Invoice invoice) =>
        JsonSerializer.Serialize(invoice, JsonOptions);

    private static string SerializeReturnInvoice()
    {
        var originalInvoice = CreateGeneralInvoice();

        var payload = new
        {
            originalInvoiceNumber = originalInvoice.InvoiceNumber,
            returnInvoiceNumber = $"RET-{originalInvoice.InvoiceNumber}",
            returnReason = "Customer request / item returned",
            returnUUID = "21a105e5-4f52-4bb3-8b8b-38cdbbb6da3c",
            type = "SalesReturn",
            invoiceDate = originalInvoice.InvoiceDate,
            paymentType = "SameAsOriginal",
            originalPaymentType = originalInvoice.PaymentType.ToString(),
            originalInvoiceType = originalInvoice.Type.ToString(),
            uniqueSerialNumber = originalInvoice.UniqueSerialNumber,
            currency = originalInvoice.Currency.ToString(),
            supplier = originalInvoice.Supplier,
            customer = originalInvoice.Customer,
            invoiceTotals = originalInvoice.InvoiceTotals,
            invoiceDetails = originalInvoice.InvoiceDetails,
            invoiceNote = originalInvoice.InvoiceNote
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static Invoice CreateIncomeInvoice()
    {
        var details = new List<InvoiceDetail>
        {
            new InvoiceDetail("LINE-1", TaxCategoryCode.Z, "Consulting Services")
            {
                Quantity = 1,
                UnitPriceBeforeTax = 100.00m,
                TotalBeforeTax = 100.00m,
                TaxAmount = 0.00m,
                TotalIncludingTax = 100.00m
            }
        };

        var totals = new InvoiceTotals
        {
            TotalVATAmount = 0.00m,
            TotalSpecialTaxAmount = 0.00m,
            TotalBeforeDiscount = 100.00m,
            TotalInvoiceAmount = 100.00m,
            TotalDiscountAmount = 0.00m,
            FinalPayableAmount = 100.00m
        };

        return CreateBaseInvoice(
            invoiceNumber: "INC-2024-001",
            uniqueSerialNumber: "b5b9a8d3-1f1e-4c55-8f6f-2f2b7a7d0001",
            invoiceDate: "2024-01-15",
            paymentType: InvoicePaymentTypeCode.LocalIncomeCash,
            supplier: CreateDefaultSupplier(),
            customer: CreateDefaultCustomer(),
            totals: totals,
            details: details,
            type: InvoiceType.Income,
            note: "Professional consulting services"
        );
    }

    private static Invoice CreateGeneralInvoice()
    {
        var details = new List<InvoiceDetail>
        {
            new InvoiceDetail("LINE-1", TaxCategoryCode.S, "Product A")
            {
                Quantity = 2,
                UnitPriceBeforeTax = 50.00m,
                TotalBeforeTax = 100.00m,
                TaxAmount = 16.00m,
                TotalIncludingTax = 116.00m
            }
        };

        var totals = new InvoiceTotals
        {
            TotalVATAmount = 16.00m,
            TotalSpecialTaxAmount = 0.00m,
            TotalBeforeDiscount = 100.00m,
            TotalInvoiceAmount = 116.00m,
            TotalDiscountAmount = 0.00m,
            FinalPayableAmount = 116.00m
        };

        return CreateBaseInvoice(
            invoiceNumber: "GEN-2024-001",
            uniqueSerialNumber: "b5b9a8d3-1f1e-4c55-8f6f-2f2b7a7d0002",
            invoiceDate: "2024-01-15",
            paymentType: InvoicePaymentTypeCode.LocalGeneralSalesCash,
            supplier: CreateDefaultSupplier(),
            customer: CreateDefaultCustomer(),
            totals: totals,
            details: details,
            type: InvoiceType.GeneralSales,
            note: "General sales invoice"
        );
    }

    private static Invoice CreateSpecialInvoice()
    {
        var details = new List<InvoiceDetail>
        {
            new InvoiceDetail("LINE-1", TaxCategoryCode.S, "Special Item")
            {
                Quantity = 1,
                UnitPriceBeforeTax = 150.00m,
                TotalBeforeTax = 150.00m,
                TaxAmount = 24.00m,
                TotalIncludingTax = 174.00m,
                SpecialTaxAmount = 0.00m
            }
        };

        var totals = new InvoiceTotals
        {
            TotalVATAmount = 24.00m,
            TotalSpecialTaxAmount = 0.00m,
            TotalBeforeDiscount = 150.00m,
            TotalInvoiceAmount = 174.00m,
            TotalDiscountAmount = 0.00m,
            FinalPayableAmount = 174.00m
        };

        return CreateBaseInvoice(
            invoiceNumber: "SPC-2024-001",
            uniqueSerialNumber: "b5b9a8d3-1f1e-4c55-8f6f-2f2b7a7d0003",
            invoiceDate: "2024-01-15",
            paymentType: InvoicePaymentTypeCode.LocalSpecialSalesCash,
            supplier: CreateDefaultSupplier(),
            customer: CreateDefaultCustomer(),
            totals: totals,
            details: details,
            type: InvoiceType.SpecialSales,
            note: "Special sales invoice"
        );
    }

    private static Invoice CreateBaseInvoice(
        string invoiceNumber,
        string uniqueSerialNumber,
        string invoiceDate,
        InvoicePaymentTypeCode paymentType,
        Supplier supplier,
        Customer customer,
        InvoiceTotals totals,
        List<InvoiceDetail> details,
        InvoiceType type,
        string note)
    {
        var invoice = new Invoice(
            invoiceNumber: invoiceNumber,
            uniqueSerialNumber: uniqueSerialNumber,
            invoiceDate: invoiceDate,
            paymentType: paymentType,
            supplier: supplier,
            customer: customer,
            invoiceTotals: totals,
            invoiceDetails: details,
            type: type)
        {
            InvoiceNote = note
        };

        return invoice;
    }

    private static Supplier CreateDefaultSupplier() =>
        new("123456789", "62010", "Default Supplier LLC");

    private static Customer CreateDefaultCustomer() =>
        new("Retail Customer");
}
