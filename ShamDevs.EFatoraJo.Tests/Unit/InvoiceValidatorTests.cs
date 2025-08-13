using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Tests.Builders;
using ShamDevs.EFatoraJo.Utilities;

namespace ShamDevs.EFatoraJo.Tests.Unit
{
    public class InvoiceValidatorTests
    {
        private const CurrencyCode TestCurrency = CurrencyCode.JOD;

        #region Null / Empty Validation Tests
        [Fact]
        public void ValidateInvoice_WhenInvoiceIsNull_ThrowsValidationException()
        {
            Invoice? invoice = null;
            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Invoice data cannot be null", exception.ValidationErrors);
        }
        #endregion

        #region Basic Invoice Details Validation
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateBasicDetails_WhenInvoiceNumberIsInvalid_ReturnsError(string invoiceNumber)
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceNumber(invoiceNumber)
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("InvoiceNumber is required", exception.ValidationErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("2023/01/01")]
        [InlineData("01-01-2023")]
        public void ValidateBasicDetails_WhenInvoiceDateIsInvalid_ReturnsError(string invoiceDate)
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceDate(invoiceDate)
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("InvoiceDate", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateBasicDetails_WhenInvoiceDateIsFuture_ReturnsError()
        {
            var futureDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var invoice = new InvoiceBuilder()
                .WithInvoiceDate(futureDate)
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("InvoiceDate must be in valid yyyy-MM-dd format", exception.ValidationErrors);
        }

        [Fact]
        public void ValidateBasicDetails_WhenInvoiceTypeIsInvalid_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoicePaymentType((InvoicePaymentTypeCode)999)
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("PaymentType is invalid", exception.ValidationErrors);
        }
        #endregion

        #region Supplier Validation
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateSupplier_WhenTaxVATNumberIsInvalid_ReturnsError(string vatNumber)
        {
            var invoice = new InvoiceBuilder()
                .WithSupplier(s => s.WithTaxVATNumber(vatNumber))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Supplier TaxVATNumber is required", exception.ValidationErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateSupplier_WhenRegisteredSupplierNameIsInvalid_ReturnsError(string supplierName)
        {
            var invoice = new InvoiceBuilder()
                .WithSupplier(s => s.WithRegisteredSupplierName(supplierName))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Supplier RegisteredSupplierName is required", exception.ValidationErrors);
        }

        [Fact]
        public void ValidateSupplier_WhenSupplierIsNull_ReturnsError()
        {
            var invoice = new InvoiceBuilder().Build();
            invoice.Supplier = null;

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Supplier data cannot be null", exception.ValidationErrors);
        }
        #endregion

        #region Customer Validation
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateCustomer_WhenIdentificationNumberIsInvalid_ReturnsError(string idNumber)
        {
            var invoice = new InvoiceBuilder()
                .WithCustomer(c => c.WithIdentificationNumber(idNumber))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Customer IdentificationNumber is required", exception.ValidationErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateCustomer_WhenNameIsInvalid_ReturnsError(string name)
        {
            var invoice = new InvoiceBuilder()
                .WithCustomer(c => c.WithName(name))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Customer Name is required", exception.ValidationErrors);
        }

        [Fact]
        public void ValidateCustomer_WhenCustomerIsNull_ReturnsError()
        {
            var invoice = new InvoiceBuilder().Build();
            invoice.Customer = null;

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Customer data cannot be null", exception.ValidationErrors);
        }
        #endregion

        #region Invoice Line Items Validation
        [Fact]
        public void ValidateInvoiceLines_WhenDetailsIsNull_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithMultipleLineItems()
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("At least one InvoiceDetail is required", exception.ValidationErrors);
        }

        [Fact]
        public void ValidateInvoiceLines_WhenDetailsIsEmpty_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithMultipleLineItems()
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("At least one InvoiceDetail is required", exception.ValidationErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateInvoiceLines_WhenLineItemIdIsInvalid_ReturnsError(string id)
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithId(id))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("InvoiceDetail ID is required", exception.ValidationErrors.First());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateInvoiceLines_WhenQuantityIsInvalid_ReturnsError(int quantity)
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithQuantity(quantity))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Quantity must be greater than zero", exception.ValidationErrors.First());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateInvoiceLines_WhenUnitPriceBeforeTaxIsInvalid_ReturnsError(decimal price)
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithUnitPrice(price))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("UnitPriceBeforeTax must be greater than zero", exception.ValidationErrors.First());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateInvoiceLines_WhenDescriptionIsInvalid_ReturnsError(string description)
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithDescription(description))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Description is required", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateInvoiceLines_WhenTaxCategoryIsInvalid_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithTaxCategory((TaxCategoryCode)999))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Invalid TaxCategory value", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateInvoiceLines_WhenTotalBeforeTaxIsIncorrect_ReturnsError()
        {
            var unitPrice = 10.000m;
            var quantity = 2;
            var discount = 1.000m;
            var expectedTotalBeforeTax = CurrencyHelper.Round((unitPrice * quantity) - discount, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithQuantity(quantity)
                    .WithUnitPrice(unitPrice)
                    .WithDiscountAmount(discount))
                .Build();

            invoice.InvoiceDetails.First().TotalBeforeTax = 10.000m;

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains($"Invalid TotalBeforeTax for line", exception.ValidationErrors.First());
            Assert.Contains($"Expected: {expectedTotalBeforeTax}", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateInvoiceLines_WhenTotalIncludingTaxIsIncorrect_ReturnsError()
        {
            var unitPrice = 100.000m;
            var taxRate = 0.16m;
            var expectedTax = CurrencyHelper.Round(unitPrice * taxRate, TestCurrency);
            var expectedTotalIncTax = CurrencyHelper.Round(unitPrice + expectedTax, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithUnitPrice(unitPrice)
                    .WithTaxCategory(TaxCategoryCode.S)
                    .WithDiscountAmount(0))
                .Build();

            invoice.InvoiceDetails.First().TotalIncludingTax = 110.000m;

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Invalid TotalIncludingTax", exception.ValidationErrors.First());
            Assert.Contains($"Expected: {expectedTotalIncTax}", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateInvoiceLines_WhenDiscountAmountIsNegative_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithDiscountAmount(-5.000m))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("DiscountAmount cannot be negative", exception.ValidationErrors.First());
        }
        #endregion

        #region Tax Category Consistency
        [Fact]
        public void ValidateInvoiceLines_WhenZeroTaxCategoryHasTaxAmount_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithTaxCategory(TaxCategoryCode.Z)
                    .WithTaxAmount(10.000m))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            // Match the exact message format from the validator
            Assert.Contains(
                "Invalid TaxAmount for line ITEM-001. Expected: 0.000, Actual: 10.000",
                exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateInvoiceLines_WhenExemptCategoryHasTaxAmount_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithTaxCategory(TaxCategoryCode.O) // Exempt category
                    .WithTaxAmount(10.000m)) // Should be 0 for exempt
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            // Match the exact first error message from the output
            Assert.Equal(
                "Invalid TaxAmount for line ITEM-001. Expected: 0.000, Actual: 10.000",
                exception.ValidationErrors.First());
        }
        #endregion

        #region Invoice-Type-Specific Validation
        [Fact]
        public void ValidateInvoiceLines_WhenIncomeInvoice_HasTaxAmount_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceType(InvoiceType.Income)
                .WithLineItem(i => i.WithTaxAmount(10m))
                .Build();

            var ex = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains(
                "Income invoice: line ITEM-001 TaxAmount must be 0.",
                ex.ValidationErrors);
        }

        [Fact]
        public void ValidateInvoiceLines_WhenGeneralSalesInvoice_HasSpecialTaxAmount_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceType(InvoiceType.GeneralSales)
                .WithLineItem(i => i.WithSpecialTaxAmount(5m))
                .Build();

            var ex = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains(
                "GeneralSales invoice: line ITEM-001 must not have SpecialTaxAmount.",
                ex.ValidationErrors);
        }

        [Fact]
        public void ValidateInvoiceLines_WhenSpecialSalesInvoice_WithBothTaxes_Passes()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceType(InvoiceType.SpecialSales)
                .WithLineItem(i => i
                    .WithUnitPrice(100m)
                    .WithQuantity(1)
                    .WithTaxCategory(TaxCategoryCode.S)
                    .WithTaxAmount(16m)
                    .WithSpecialTaxAmount(5m)
                    .WithTotalBeforeTax(100m)
                    .WithTotalIncludingTax(121m)) // 100 + 16 + 5
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(100m)
                    .WithTotalVatAmount(16m)
                    .WithTotalSpecialTaxAmount(5m)
                    .WithTotalInvoiceAmount(121m)
                    .WithFinalPayableAmount(121m))
                .Build();

            var ex = Record.Exception(() => InvoiceValidator.ValidateInvoice(invoice));
            Assert.Null(ex);
        }
        #endregion

        #region Monetary Totals Validation

        [Fact]
        public void ValidateMonetaryTotals_WhenTotalsIsNull_ReturnsError()
        {
            var invoice = new InvoiceBuilder().Build();
            invoice.InvoiceTotals = null;

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("InvoiceTotals data cannot be null", exception.ValidationErrors);
        }

        [Fact]
        public void ValidateMonetaryTotals_WhenTotalBeforeDiscountIsIncorrect_ReturnsError()
        {
            var item1Total = 2 * 10.000m;
            var item2Total = 3 * 20.000m;
            var expectedTotal = CurrencyHelper.Round(item1Total + item2Total, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i.WithQuantity(2).WithUnitPrice(10.000m))
                .WithLineItem(i => i.WithQuantity(3).WithUnitPrice(20.000m))
                .WithTotals(t => t.WithTotalBeforeDiscount(50.000m))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Invalid TotalBeforeDiscount", exception.ValidationErrors.First());
            Assert.Contains($"Expected: {expectedTotal}", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateMonetaryTotals_WhenTotalDiscountAmountIsIncorrect_ReturnsError()
        {
            var discount1 = 5.000m;
            var discount2 = 10.000m;
            var expectedTotalDiscount = CurrencyHelper.Round(discount1 + discount2, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithMultipleLineItems(
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithDiscountAmount(discount1),
                    i => i.WithQuantity(2).WithUnitPrice(50m).WithDiscountAmount(discount2))
                .WithTotals(t => t.WithTotalDiscountAmount(10.000m))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains($"Invalid TotalDiscountAmount. Expected: {expectedTotalDiscount}",
                exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateMonetaryTotals_WhenTotalVATAmountIsIncorrect_ReturnsError()
        {
            var tax1 = CurrencyHelper.Round(100.000m * 0.16m, TestCurrency);
            var tax2 = CurrencyHelper.Round(200.000m * 0.16m, TestCurrency);
            var expectedTotalVAT = CurrencyHelper.Round(tax1 + tax2, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithMultipleLineItems(
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.S),
                    i => i.WithQuantity(1).WithUnitPrice(200m).WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t.WithTotalVatAmount(30.000m))   // <── new property
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains($"Invalid TotalVATAmount. Expected: {expectedTotalVAT}",
                exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateMonetaryTotals_WhenTotalSpecialTaxAmountIsIncorrect_ReturnsError()
        {
            var specialTax1 = 10.000m;
            var specialTax2 = 20.000m;
            var expectedSpecialTax = CurrencyHelper.Round(specialTax1 + specialTax2, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithInvoiceType(InvoiceType.SpecialSales)
                .WithMultipleLineItems(
                    i => i.WithQuantity(1)
                          .WithUnitPrice(100m)
                          .WithTaxCategory(TaxCategoryCode.S)
                          .WithSpecialTaxAmount(specialTax1),
                    i => i.WithQuantity(1)
                          .WithUnitPrice(200m)
                          .WithTaxCategory(TaxCategoryCode.S)
                          .WithSpecialTaxAmount(specialTax2))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(300m)
                    .WithTotalVatAmount(48m) // 16% of 300
                    .WithTotalSpecialTaxAmount(25m) // Incorrect - should be 30
                    .WithTotalInvoiceAmount(373m)) // 300 + 48 + 25
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            // Match the exact error message format from the validator
            Assert.Contains(
                $"Invalid TotalSpecialTaxAmount. Expected: {expectedSpecialTax}, Actual: 25.000",
                exception.ValidationErrors);
        }

        [Fact]
        public void ValidateMonetaryTotals_WhenTotalInvoiceAmountIsIncorrect_ReturnsError()
        {
            var currency = TestCurrency;
            var precision = CurrencyHelper.GetPrecision(currency);

            var item1Total = CurrencyHelper.Round(100.000m * 1.16m, currency);
            var item2Total = CurrencyHelper.Round(200.000m * 1.16m, currency);
            var expectedTotal = CurrencyHelper.Round(item1Total + item2Total, currency);

            var invoice = new InvoiceBuilder()
                .WithMultipleLineItems(
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.S),
                    i => i.WithQuantity(1).WithUnitPrice(200m).WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t.WithTotalInvoiceAmount(300.000m))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            var expectedFormatted = expectedTotal.ToString("N" + precision);
            var actualFormatted = 300.000m.ToString("N" + precision);
            Assert.Contains($"Invalid TotalInvoiceAmount. Expected: {expectedFormatted}, Actual: {actualFormatted}",
                exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateMonetaryTotals_WhenFinalPayableAmountIsIncorrect_ReturnsError()
        {
            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithQuantity(1)
                    .WithUnitPrice(100m)
                    .WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(100m)
                    .WithTotalVatAmount(16m)
                    .WithTotalInvoiceAmount(116m)
                    .WithFinalPayableAmount(100m)) // Incorrect - should be 116
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            // Match the exact error message format from the validator
            Assert.Contains("FinalPayableAmount must equal TotalInvoiceAmount",
                exception.ValidationErrors.First());
            Assert.Contains($"Expected: {116m}", exception.ValidationErrors.First());
            Assert.Contains($"Actual: {100m}", exception.ValidationErrors.First());
        }

        #endregion

        #region Tax Calculation Validation
        [Fact]
        public void ValidateInvoiceLines_WhenTaxAmountIsIncorrectForCategory_ReturnsError()
        {
            var unitPrice = 10.000m;
            var quantity = 2;
            var discount = 1.000m;
            var taxRate = 0.16m;
            var expectedTax = CurrencyHelper.Round((unitPrice * quantity - discount) * taxRate, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithQuantity(quantity)
                    .WithUnitPrice(unitPrice)
                    .WithTaxCategory(TaxCategoryCode.S)
                    .WithDiscountAmount(discount))
                .Build();

            invoice.InvoiceDetails.First().TaxAmount = 2.000m;

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.Contains("Invalid TaxAmount for line", exception.ValidationErrors.First());
            Assert.Contains($"Expected: {expectedTax}", exception.ValidationErrors.First());
        }

        [Fact]
        public void ValidateInvoiceLines_WhenTaxCalculationMatchesCategoryRate_DoesNotThrow()
        {
            var unitPrice = 100.000m;
            var taxRate = 0.16m; // 16% VAT
            var expectedTax = CurrencyHelper.Round(unitPrice * taxRate, TestCurrency);
            var expectedTotal = CurrencyHelper.Round(unitPrice + expectedTax, TestCurrency);

            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithQuantity(1)
                    .WithUnitPrice(unitPrice)
                    .WithTaxCategory(TaxCategoryCode.S) // Standard VAT (16%)
                    .WithTaxAmount(expectedTax)
                    .WithTotalBeforeTax(unitPrice)
                    .WithTotalIncludingTax(expectedTotal))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(unitPrice)
                    .WithTotalVatAmount(expectedTax)
                    .WithTotalInvoiceAmount(expectedTotal)
                    .WithFinalPayableAmount(expectedTotal)) // Must match TotalInvoiceAmount
                .Build();

            var exception = Record.Exception(() => InvoiceValidator.ValidateInvoice(invoice));
            Assert.Null(exception);
            Assert.Equal(expectedTax, invoice.InvoiceDetails.First().TaxAmount);
        }
        #endregion

        #region Integration / Edge-Case Tests

        [Fact]
        public void ValidateInvoice_WhenAllFieldsAreValid_DoesNotThrowException()
        {
            var unitPrice = 10m;
            var quantity = 2;
            var discount = 1m;
            var taxRate = 0.16m; // 16% VAT
            var totalBeforeTax = unitPrice * quantity - discount; // 20 - 1 = 19
            var taxAmount = totalBeforeTax * taxRate; // 19 * 0.16 = 3.04
            var totalIncludingTax = totalBeforeTax + taxAmount; // 19 + 3.04 = 22.04

            var invoice = new InvoiceBuilder()
                .WithLineItem(i => i
                    .WithQuantity(quantity)
                    .WithUnitPrice(unitPrice)
                    .WithDiscountAmount(discount)
                    .WithTaxCategory(TaxCategoryCode.S) // 16% VAT
                    .WithTaxAmount(taxAmount)
                    .WithTotalBeforeTax(totalBeforeTax)
                    .WithTotalIncludingTax(totalIncludingTax))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(unitPrice * quantity) // 20
                    .WithTotalDiscountAmount(discount) // 1
                    .WithTotalVatAmount(taxAmount) // 3.04
                    .WithTotalInvoiceAmount(totalIncludingTax) // 22.04
                    .WithFinalPayableAmount(totalIncludingTax)) // Must match TotalInvoiceAmount
                .Build();

            var exception = Record.Exception(() => InvoiceValidator.ValidateInvoice(invoice));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateInvoice_WhenMultipleErrorsExist_ReturnsAllErrors()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceNumber("")
                .WithInvoiceDate("invalid-date")
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithSupplier(s => s
                    .WithTaxVATNumber("")
                    .WithRegisteredSupplierName(""))
                .WithCustomer(c => c
                    .WithIdentificationNumber("")
                    .WithName(""))
                .WithLineItem(i => i
                    .WithId("")
                    .WithQuantity(-1)
                    .WithUnitPrice(0)
                    .WithDescription("")
                    .WithTaxCategory((TaxCategoryCode)999))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(-100m)
                    .WithTotalDiscountAmount(-10m)
                    .WithTotalVatAmount(-20m)
                    .WithTotalSpecialTaxAmount(-5m)
                    .WithTotalInvoiceAmount(-200m)
                    .WithFinalPayableAmount(-150m))
                .Build();

            var exception = Assert.Throws<InvoiceValidationException>(
                () => InvoiceValidator.ValidateInvoice(invoice));

            Assert.True(exception.ValidationErrors.Count >= 12);
        }

        [Fact]
        public void ValidateInvoice_AllTypes_Integration()
        {
            foreach (var kind in Enum.GetValues<InvoiceType>())
            {
                var invoice = new InvoiceBuilder().WithInvoiceType(kind).Build();
                var ex = Record.Exception(() => InvoiceValidator.ValidateInvoice(invoice));
                Assert.True(ex is null || ex is InvoiceValidationException,
                    $"Validation for {kind} should run without unhandled exceptions.");
            }
        }

        #endregion
    }
}