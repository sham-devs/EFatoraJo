using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShamDevs.EFatoraJo.Utilities
{
    public static class InvoiceValidator
    {
        public static void ValidateInvoice(Invoice? invoice)
        {
            if (invoice == null)
                throw new InvoiceValidationException(new List<string> { "Invoice data cannot be null" });

            var errors = new List<string>();

            errors.AddRange(ValidateBasicDetails(invoice));
            errors.AddRange(ValidateSupplier(invoice.Supplier));
            errors.AddRange(ValidateCustomer(invoice.Customer));
            errors.AddRange(ValidateInvoiceLines(invoice.InvoiceDetails, invoice.Currency, invoice.Type));
            errors.AddRange(ValidateMonetaryTotals(invoice.InvoiceTotals, invoice.InvoiceDetails, invoice.Currency, invoice.Type));
            errors.AddRange(ValidateTypeRules(invoice));

            if (errors.Any())
                throw new InvoiceValidationException(errors);
        }

        #region Basic details
        private static List<string> ValidateBasicDetails(Invoice invoice)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
                errors.Add("InvoiceNumber is required");

            if (string.IsNullOrWhiteSpace(invoice.InvoiceDate))
                errors.Add("InvoiceDate is required");
            else if (!IsValidInvoiceDate(invoice.InvoiceDate))
                errors.Add("InvoiceDate must be in valid yyyy-MM-dd format");

            if (!Enum.IsDefined(typeof(InvoicePaymentTypeCode), invoice.PaymentType))
                errors.Add("PaymentType is invalid");
            else if (string.IsNullOrWhiteSpace(invoice.InvoiceTypeCode))
                errors.Add("PaymentType is required");

            return errors;
        }
        #endregion

        #region Supplier / Customer
        private static List<string> ValidateSupplier(Supplier supplier)
        {
            var errors = new List<string>();

            if (supplier == null)
            {
                errors.Add("Supplier data cannot be null");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(supplier.TaxVATNumber))
                errors.Add("Supplier TaxVATNumber is required");

            if (string.IsNullOrWhiteSpace(supplier.RegisteredSupplierName))
                errors.Add("Supplier RegisteredSupplierName is required");

            return errors;
        }

        private static List<string> ValidateCustomer(Customer customer)
        {
            var errors = new List<string>();

            if (customer == null)
            {
                errors.Add("Customer data cannot be null");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(customer.IdentificationNumber))
                errors.Add("Customer IdentificationNumber is required");

            if (string.IsNullOrWhiteSpace(customer.Name))
                errors.Add("Customer Name is required");

            return errors;
        }
        #endregion

        #region Invoice lines
        private static List<string> ValidateInvoiceLines(List<InvoiceDetail> details, CurrencyCode currency, InvoiceType invoiceType)
        {
            var errors = new List<string>();

            if (details == null || !details.Any())
            {
                errors.Add("At least one InvoiceDetail is required");
                return errors;
            }

            foreach (var detail in details)
            {
                int lineNo = details.IndexOf(detail) + 1;

                if (string.IsNullOrWhiteSpace(detail.ID))
                    errors.Add($"InvoiceDetail ID is required for line {lineNo}");

                if (detail.Quantity <= 0)
                    errors.Add($"Quantity must be greater than zero for line {detail.ID ?? "N/A"}");

                if (detail.UnitPriceBeforeTax <= 0)
                    errors.Add($"UnitPriceBeforeTax must be greater than zero for line {detail.ID ?? "N/A"}");

                if (string.IsNullOrWhiteSpace(detail.Description))
                    errors.Add($"Description is required for line {detail.ID ?? "N/A"}");

                if (!Enum.IsDefined(typeof(TaxCategoryCode), detail.TaxCategory))
                    errors.Add($"Invalid TaxCategory value for line {detail.ID ?? "N/A"}");

                // Validate TaxRate against TaxCategory
                decimal expectedTaxRate = detail.TaxPercent / 100m;

                if (detail.TaxCategory == TaxCategoryCode.Z || detail.TaxCategory == TaxCategoryCode.O)
                {
                    if (detail.TaxRate != 0m)
                    {
                        errors.Add($"TaxRate must be 0 for tax category {detail.TaxCategory} in line {detail.ID ?? "N/A"}");
                    }
                }
                else if (detail.TaxRate != expectedTaxRate)
                {
                    errors.Add($"Invalid TaxRate for line {detail.ID ?? "N/A"}. Expected: {expectedTaxRate}, Actual: {detail.TaxRate}");
                }

                decimal discount = detail.DiscountAmount ?? 0m;
                if (discount < 0)
                    errors.Add($"DiscountAmount cannot be negative for line {detail.ID ?? "N/A"}");

                decimal expectedTotalBeforeTax = CurrencyHelper.Round(
                    (detail.UnitPriceBeforeTax * detail.Quantity) - discount,
                    currency);

                decimal expectedTax = CurrencyHelper.Round(
                    expectedTotalBeforeTax * detail.TaxRate,
                    currency);

                // Invoice type specific validations
                decimal expectedTotalIncTax;
                switch (invoiceType)
                {
                    case InvoiceType.SpecialSales:
                        expectedTotalIncTax = CurrencyHelper.Round(
                            expectedTotalBeforeTax + expectedTax + (detail.SpecialTaxAmount ?? 0m),
                            currency);
                        break;

                    case InvoiceType.GeneralSales:
                        if (detail.SpecialTaxAmount.HasValue && detail.SpecialTaxAmount != 0m)
                        {
                            errors.Add($"GeneralSales invoice: line {detail.ID} must not have SpecialTaxAmount");
                        }
                        expectedTotalIncTax = CurrencyHelper.Round(
                            expectedTotalBeforeTax + expectedTax,
                            currency);
                        break;

                    case InvoiceType.Income:
                        if (detail.TaxAmount != 0m || (detail.SpecialTaxAmount.HasValue && detail.SpecialTaxAmount != 0m))
                        {
                            errors.Add($"Income invoice: line {detail.ID} must not have any taxes");
                        }
                        expectedTotalIncTax = CurrencyHelper.Round(
                            expectedTotalBeforeTax,
                            currency);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(invoiceType), invoiceType, null);
                }

                if (detail.TotalBeforeTax != expectedTotalBeforeTax)
                    errors.Add($"Invalid TotalBeforeTax for line {detail.ID ?? "N/A"}. Expected: {expectedTotalBeforeTax}, Actual: {detail.TotalBeforeTax}");

                if (detail.TaxAmount != expectedTax && invoiceType != InvoiceType.Income)
                    errors.Add($"Invalid TaxAmount for line {detail.ID ?? "N/A"}. Expected: {expectedTax}, Actual: {detail.TaxAmount}");

                if (detail.TotalIncludingTax != expectedTotalIncTax)
                    errors.Add($"Invalid TotalIncludingTax for line {detail.ID ?? "N/A"}. Expected: {expectedTotalIncTax}, Actual: {detail.TotalIncludingTax}");
            }

            return errors;
        }

        #endregion

        #region Monetary totals
        private static List<string> ValidateMonetaryTotals(InvoiceTotals totals, List<InvoiceDetail> details, CurrencyCode currency, InvoiceType invoiceType)
        {
            var errors = new List<string>();

            if (totals == null)
            {
                errors.Add("InvoiceTotals data cannot be null");
                return errors;
            }

            if (details == null || !details.Any())
            {
                errors.Add("Cannot validate totals without invoice details");
                return errors;
            }

            // Calculate base amounts
            decimal calculatedTotalBeforeDiscount = CurrencyHelper.Round(
                details.Sum(d => d.UnitPriceBeforeTax * d.Quantity),
                currency);

            decimal calculatedTotalDiscountAmount = CurrencyHelper.Round(
                details.Sum(d => d.DiscountAmount ?? 0m),
                currency);

            decimal taxableAmount = calculatedTotalBeforeDiscount - calculatedTotalDiscountAmount;

            // Calculate tax amounts based on invoice type
            decimal calculatedTotalVATAmount = invoiceType == InvoiceType.Income
                ? 0m
                : CurrencyHelper.Round(details.Sum(d => d.TaxAmount), currency);

            decimal calculatedTotalSpecialTaxAmount = invoiceType == InvoiceType.SpecialSales
                ? CurrencyHelper.Round(details.Sum(d => d.SpecialTaxAmount ?? 0m), currency)
                : 0m;

            decimal calculatedTotalInvoiceAmount = CurrencyHelper.Round(
                taxableAmount + calculatedTotalVATAmount + calculatedTotalSpecialTaxAmount,
                currency);

            // Invoice type specific validations
            switch (invoiceType)
            {
                case InvoiceType.Income:
                    if (totals.TotalVATAmount != 0m)
                        errors.Add("Income invoice must not have VAT amount");
                    if (totals.TotalSpecialTaxAmount != 0m)
                        errors.Add("Income invoice must not have special tax amount");
                    break;

                case InvoiceType.GeneralSales:
                    if (totals.TotalSpecialTaxAmount != 0m)
                        errors.Add("GeneralSales invoice must not have special tax amount");
                    break;
            }

            // Validate totals
            if (totals.TotalBeforeDiscount != calculatedTotalBeforeDiscount)
                errors.Add($"Invalid TotalBeforeDiscount. Expected: {calculatedTotalBeforeDiscount}, Actual: {totals.TotalBeforeDiscount}");

            if (totals.TotalDiscountAmount != calculatedTotalDiscountAmount)
                errors.Add($"Invalid TotalDiscountAmount. Expected: {calculatedTotalDiscountAmount}, Actual: {totals.TotalDiscountAmount}");

            if (totals.TotalVATAmount != calculatedTotalVATAmount)
                errors.Add($"Invalid TotalVATAmount. Expected: {calculatedTotalVATAmount}, Actual: {totals.TotalVATAmount}");

            if (totals.TotalSpecialTaxAmount != calculatedTotalSpecialTaxAmount)
                errors.Add($"Invalid TotalSpecialTaxAmount. Expected: {calculatedTotalSpecialTaxAmount}, Actual: {totals.TotalSpecialTaxAmount}");

            if (totals.TotalInvoiceAmount != calculatedTotalInvoiceAmount)
                errors.Add($"Invalid TotalInvoiceAmount. Expected: {calculatedTotalInvoiceAmount}, Actual: {totals.TotalInvoiceAmount}");

            // Final payable amount should equal total invoice amount
            if (totals.FinalPayableAmount != totals.TotalInvoiceAmount)
            {
                errors.Add($"FinalPayableAmount must equal TotalInvoiceAmount. Expected: {totals.TotalInvoiceAmount}, Actual: {totals.FinalPayableAmount}");
            }

            return errors;
        }
        #endregion

        #region Type-specific rules
        private static List<string> ValidateTypeRules(Invoice inv)
        {
            var errors = new List<string>();

            switch (inv.Type)
            {
                case InvoiceType.Income:
                    // must not contain any tax amounts
                    foreach (var d in inv.InvoiceDetails)
                    {
                        if (d.TaxAmount != 0m)
                            errors.Add($"Income invoice: line {d.ID} TaxAmount must be 0.");
                        if (d.SpecialTaxAmount.HasValue && d.SpecialTaxAmount != 0m)
                            errors.Add($"Income invoice: line {d.ID} SpecialTaxAmount must be 0.");
                    }
                    break;

                case InvoiceType.GeneralSales:
                    // must have only VAT (no SpecialTax)
                    foreach (var d in inv.InvoiceDetails)
                    {
                        if (d.SpecialTaxAmount.HasValue && d.SpecialTaxAmount != 0m)
                            errors.Add($"GeneralSales invoice: line {d.ID} must not have SpecialTaxAmount.");
                    }
                    break;

                case InvoiceType.SpecialSales:
                    // must have both VAT and special tax
                    foreach (var d in inv.InvoiceDetails)
                    {
                        if (!d.SpecialTaxAmount.HasValue || d.SpecialTaxAmount == 0m)
                            errors.Add($"SpecialSales invoice: line {d.ID} SpecialTaxAmount is required.");
                    }
                    break;

                default:
                    errors.Add($"Unknown InvoiceType value: {inv.Type}");
                    break;
            }

            return errors;
        }
        #endregion

        #region Helpers
        private static bool IsValidInvoiceDate(string dateString)
        {
            return DateTime.TryParseExact(dateString, "yyyy-MM-dd",
                       CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                   date <= DateTime.Today;
        }
        #endregion
    }
}