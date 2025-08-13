using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShamDevs.EFatoraJo.Utilities
{
    public class RandomInvoiceGenerator
    {
        private static readonly Random random = new Random();
        private static readonly IdentificationType[] identificationTypes =
            { IdentificationType.NIN, IdentificationType.PN, IdentificationType.TN };
        private static readonly string[] itemDescriptions = {
            "Laptop", "Monitor", "Keyboard", "Mouse", "Desk",
            "Chair", "Printer", "Software License", "Webcam",
            "Headphones", "Notebook", "Pen", "Stapler", "Paper"
        };
        private static readonly string[] returnReasons = { "Defective", "Wrong Item", "Customer Changed Mind", "Late Delivery" };

        public static Invoice GenerateRandomInvoice(
             Supplier? supplierInfo = null,
             InvoicePaymentTypeCode? invoicePaymentType = null,
             CurrencyCode? currency = null,
             InvoiceType? invoiceTpe = null)
        {
            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{random.Next(1000, 9999)}";
            var uniqueSerialNumber = Guid.NewGuid().ToString();
            var invoiceDate = DateTime.Now.AddDays(-random.Next(0, 30)).ToString("yyyy-MM-dd");
            var supplier = supplierInfo ?? GenerateRandomSupplier();
            var customer = GenerateRandomCustomer();
            var currencyCode = currency ?? CurrencyCode.JOD;
            var type = invoiceTpe ?? (InvoiceType)random.Next(0, 3);
            var invoiceDetails = GenerateRandomInvoiceDetails(random.Next(2, 6), currencyCode, type);
            var invoiceTotals = new InvoiceTotals();

            var invoice = new Invoice(
                invoiceNumber,
                uniqueSerialNumber,
                invoiceDate,
                invoicePaymentType ?? GetRandomInvoicePaymentType(),
                supplier,
                customer,
                invoiceTotals,
                invoiceDetails)
            {
                InvoiceNote = random.Next(0, 100) > 70 ? "Urgent processing required" : null,
                Currency = currencyCode,
            };

            CalculateInvoiceTotals(invoice);
            return invoice;
        }

        public static SalesReturnInvoice GenerateRandomSalesReturnInvoice(
            Invoice? originalInvoice = null,
            Supplier? supplierInfo = null,
            CurrencyCode? currency = null)
        {
            var original = originalInvoice ?? GenerateRandomInvoice(supplierInfo, currency: currency);
            var returnInvoiceNumber = $"RINV-{DateTime.Now:yyyyMMdd}-{random.Next(1000, 9999)}";
            var returnUniqueSerialNumber = Guid.NewGuid().ToString();
            var returnInvoiceDate = DateTime.Now.ToString("yyyy-MM-dd");
            var returnReason = returnReasons[random.Next(returnReasons.Length)];

            return new SalesReturnInvoice(
                returnInvoiceNumber,
                original,
                returnUniqueSerialNumber,
                returnInvoiceDate,
                returnReason);
        }

        private static Supplier GenerateRandomSupplier()
        {
            return new Supplier(
                $"JO{random.Next(10000000, 99999999)}",
                $"SRC-{random.Next(1000, 9999)}",
                $"Supplier {random.Next(1, 100)} LLC");
        }

        private static Customer GenerateRandomCustomer()
        {
            return new Customer($"Customer {random.Next(1, 100)}")
            {
                IdentificationNumber = random.Next(10000000, 99999999).ToString(),
                IdentificationType = identificationTypes[random.Next(identificationTypes.Length)],
                PostalCode = random.Next(11110, 99998).ToString(),
                PhoneNumber = $"07{random.Next(10000000, 99999999)}"
            };
        }

        private static List<InvoiceDetail> GenerateRandomInvoiceDetails(int count, CurrencyCode currency, InvoiceType kind)
        {
            var details = new List<InvoiceDetail>();
            var taxCats = new[] { TaxCategoryCode.S, TaxCategoryCode.S8, TaxCategoryCode.S5,
                          TaxCategoryCode.O, TaxCategoryCode.Z };

            for (int i = 0; i < count; i++)
            {
                var taxCategory = kind switch
                {
                    InvoiceType.Income => TaxCategoryCode.Z,
                    InvoiceType.SpecialSales => random.Next(0, 3) switch
                    {
                        0 => TaxCategoryCode.S,
                        1 => TaxCategoryCode.S8,
                        _ => TaxCategoryCode.S5
                    },
                    _ => taxCats[random.Next(taxCats.Length)]
                };

                var taxRate = kind == InvoiceType.Income ? 0m : taxCategory.GetTaxPercent() / 100m;

                var unitPrice = GetRealisticUnitPrice(currency);
                var qty = random.Next(1, 10);
                var discount = random.Next(0, 100) > 80
                                  ? CurrencyHelper.Round(unitPrice * qty * random.Next(5, 21) / 100m, currency)
                                  : 0m;

                decimal baseAmount = CurrencyHelper.Round(unitPrice * qty, currency);
                decimal taxable = CurrencyHelper.Round(baseAmount - discount, currency);
                decimal taxAmount = kind == InvoiceType.Income ? 0m
                                       : CurrencyHelper.Round(taxable * taxRate, currency);

                decimal specialTax = kind == InvoiceType.SpecialSales
                                       ? CurrencyHelper.Round(taxable * random.Next(2, 11) / 100m, currency)
                                       : 0m;

                decimal inclTax = CurrencyHelper.Round(taxable + taxAmount + specialTax, currency);

                details.Add(new InvoiceDetail($"ITEM-{random.Next(1000, 9999)}",
                                              taxCategory,
                                              itemDescriptions[random.Next(itemDescriptions.Length)])
                {
                    Quantity = qty,
                    UnitPriceBeforeTax = unitPrice,
                    DiscountAmount = discount == 0m ? (decimal?)null : discount,
                    TotalBeforeTax = taxable,
                    TaxAmount = taxAmount,
                    SpecialTaxAmount = specialTax == 0m ? (decimal?)null : specialTax,
                    TotalIncludingTax = inclTax
                });
            }
            return details;
        }

        private static decimal GetRealisticUnitPrice(CurrencyCode currency)
        {
            decimal[] pricePoints = {
                49.999m, 99.999m, 149.999m, 199.999m, 249.999m,
                299.999m, 349.999m, 399.999m, 449.999m, 499.999m,
                59.999m, 79.999m, 119.999m, 159.999m, 179.999m
            };

            decimal basePrice = pricePoints[random.Next(pricePoints.Length)];

            decimal convertedPrice = currency == CurrencyCode.JOD
                ? basePrice
                : basePrice * (currency == CurrencyCode.USD ? 1.41m : 1.5m);

            return CurrencyHelper.Round(convertedPrice, currency);
        }

        private static InvoicePaymentTypeCode GetRandomInvoicePaymentType()
        {
            var values = Enum.GetValues(typeof(InvoicePaymentTypeCode));
            return (InvoicePaymentTypeCode)values.GetValue(random.Next(values.Length));
        }

        private static decimal GetRealisticUnitPrice()
        {
            decimal[] pricePoints = {
                49.999m, 99.999m, 149.999m, 199.999m, 249.999m,
                299.999m, 349.999m, 399.999m, 449.999m, 499.999m,
                59.999m, 79.999m, 119.999m, 159.999m, 179.999m
            };

            return pricePoints[random.Next(pricePoints.Length)];
        }

        private static void CalculateInvoiceTotals(Invoice invoice)
        {
            decimal totalBeforeDiscount = invoice.InvoiceDetails.Sum(d =>
                CurrencyHelper.Round(d.UnitPriceBeforeTax * d.Quantity, invoice.Currency));

            decimal totalDiscount = invoice.InvoiceDetails.Sum(d => d.DiscountAmount ?? 0m);

            decimal totalVAT = invoice.InvoiceDetails.Sum(d => d.TaxAmount);

            decimal totalSpecialTax = invoice.InvoiceDetails.Sum(d => d.SpecialTaxAmount ?? 0m);

            decimal taxableBase = totalBeforeDiscount - totalDiscount;

            var totals = invoice.InvoiceTotals;
            totals.TotalBeforeDiscount = CurrencyHelper.Round(totalBeforeDiscount, invoice.Currency);
            totals.TotalDiscountAmount = CurrencyHelper.Round(totalDiscount, invoice.Currency);
            totals.TotalVATAmount = CurrencyHelper.Round(totalVAT, invoice.Currency);
            totals.TotalSpecialTaxAmount = CurrencyHelper.Round(totalSpecialTax, invoice.Currency);
            totals.TotalInvoiceAmount = CurrencyHelper.Round(taxableBase + totalVAT + totalSpecialTax, invoice.Currency);
            totals.FinalPayableAmount = CurrencyHelper.Round(taxableBase + totalVAT + totalSpecialTax, invoice.Currency);
        }
    }
}