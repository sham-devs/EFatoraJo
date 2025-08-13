using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Services;
using ShamDevs.EFatoraJo.Tests.Builders;

namespace ShamDevs.EFatoraJo.Tests.Unit
{
    public class InvoiceGeneratorServiceTests
    {

        [Fact]
        public void GenerateUBL21_WithMinimalValidInvoice_GeneratesValidUBL()
        {
            // Arrange
            var invoice = new InvoiceBuilder()
                .WithBasicDetails("INV-001", "2023-01-01", InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithSupplier(s => s.WithTaxVATNumber("JO123456789"))
                .WithCustomer(c => c.WithName("Test Customer"))
                .WithLineItem(i => i.WithQuantity(1).WithUnitPrice(10m))
                .Build();

            // Act
            var ublInvoice = InvoiceGeneratorService.GenerateUBL21(invoice);

            // Assert
            Assert.NotNull(ublInvoice);
            Assert.Equal("INV-001", ublInvoice.ID.Value);
            Assert.Equal("JO123456789",
                ublInvoice.AccountingSupplierParty.Party.PartyTaxScheme[0].CompanyID.Value);
            Assert.Single(ublInvoice.InvoiceLine);
        }

        [Fact]
        public void GenerateUBL21_WithAllFields_GeneratesCompleteUBL()
        {
            // Arrange
            var invoice = new InvoiceBuilder()
                .WithBasicDetails("INV-002", "2023-02-01", InvoicePaymentTypeCode.LocalGeneralSalesCredit)
                .WithSupplier(s => s
                    .WithTaxVATNumber("JO987654321")
                    .WithRegisteredSupplierName("Test Supplier"))
                .WithCustomer(c => c
                    .WithName("Test Customer")
                    .WithIdentificationNumber("CUST-123")
                    .WithPhoneNumber("+962790000000"))
                .WithLineItem(i => i
                    .WithId("ITEM-001")
                    .WithQuantity(2)
                    .WithUnitPrice(15.50m)
                    .WithDescription("Test Product")
                    .WithTaxCategory(TaxCategoryCode.S))
                .Build();

            // Act
            var ublInvoice = InvoiceGeneratorService.GenerateUBL21(invoice);

            // Assert
            Assert.Equal("INV-002", ublInvoice.ID.Value);
            Assert.Equal("Test Supplier",
                ublInvoice.AccountingSupplierParty.Party.PartyLegalEntity[0].RegistrationName.Value);
            Assert.Equal("CUST-123",
                ublInvoice.AccountingCustomerParty.Party.PartyIdentification[0].ID.Value);
            Assert.Equal(31m, ublInvoice.InvoiceLine[0].LineExtensionAmount.Value);
        }

        [Fact]
        public void GenerateUBL21_WithMultipleLineItems_IncludesAllItems()
        {
            // Arrange
            var invoice = new InvoiceBuilder()
                .WithBasicDetails("INV-003", "2023-03-01", InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithMultipleLineItems(
                    i => i.WithId("ITEM-001").WithQuantity(1).WithUnitPrice(10m).WithTaxCategory(TaxCategoryCode.S),
                    i => i.WithId("ITEM-002").WithQuantity(2).WithUnitPrice(20m).WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(50m)
                    .WithTotalVatAmount(8m)        // 16% VAT on 50
                    .WithTotalSpecialTaxAmount(0m) // GeneralSales → no special tax
                    .WithTotalInvoiceAmount(58m))
                .Build();

            // Act
            var ublInvoice = InvoiceGeneratorService.GenerateUBL21(invoice);

            // Assert
            Assert.Equal(2, ublInvoice.InvoiceLine.Count);
            Assert.Equal("ITEM-001", ublInvoice.InvoiceLine[0].ID.Value);
            Assert.Equal("ITEM-002", ublInvoice.InvoiceLine[1].ID.Value);
            Assert.Equal(50m, ublInvoice.LegalMonetaryTotal.TaxExclusiveAmount.Value);
        }

        [Fact]
        public void GenerateUBL21_WithDiscounts_CalculatesCorrectAmounts()
        {
            // Arrange
            var invoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithLineItem(i => i
                    .WithQuantity(2)
                    .WithUnitPrice(10m)
                    .WithDiscountAmount(1m)
                    .WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(20m)      // 2 × 10
                    .WithTotalDiscountAmount(1m)       // explicit discount
                    .WithTotalVatAmount(3.04m)         // 16 % of (20 − 1)
                    .WithTotalSpecialTaxAmount(0m)     // GeneralSales → no special tax
                    .WithTotalInvoiceAmount(22.04m))   // (20 − 1) + 3.04
                .Build();

            // Act
            var ublInvoice = InvoiceGeneratorService.GenerateUBL21(invoice);

            // Assert
            Assert.NotNull(ublInvoice.AllowanceCharge);
            Assert.Single(ublInvoice.AllowanceCharge);
            Assert.Equal(1m, ublInvoice.AllowanceCharge[0].Amount.Value);
            Assert.Equal(19m, ublInvoice.InvoiceLine[0].LineExtensionAmount.Value);
            Assert.Equal(3.04m, ublInvoice.InvoiceLine[0].TaxTotal[0].TaxAmount.Value);
        }

        [Fact]
        public void GenerateUBL21_WithZeroDiscount_IncludesDiscountElement()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithLineItem(i => i
                    .WithQuantity(2)
                    .WithUnitPrice(10m)
                    .WithDiscountAmount(0m)
                    .WithTaxCategory(TaxCategoryCode.S))
                .Build();

            var ubl = InvoiceGeneratorService.GenerateUBL21(invoice);

            Assert.Single(ubl.AllowanceCharge);
            Assert.False(ubl.AllowanceCharge[0].ChargeIndicator.Value);
            Assert.Equal(0m, ubl.AllowanceCharge[0].Amount.Value);
            Assert.Equal(20m, ubl.InvoiceLine[0].LineExtensionAmount.Value);
        }

        [Fact]
        public void GenerateUBL21_WithTax_IncludesTaxAmounts()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithLineItem(i => i
                    .WithQuantity(1)
                    .WithUnitPrice(100m)
                    .WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(100m)
                    .WithTotalVatAmount(16m)
                    .WithTotalSpecialTaxAmount(0m)
                    .WithTotalInvoiceAmount(116m))
                .Build();

            var ubl = InvoiceGeneratorService.GenerateUBL21(invoice);

            Assert.Equal(16m, ubl.TaxTotal[0].TaxAmount.Value);
            Assert.Equal(116m, ubl.LegalMonetaryTotal.TaxInclusiveAmount.Value);
        }

        [Fact]
        public void GenerateUBL21_WithSpecialSales_IncludesVATandSpecialTax()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoiceType(InvoiceType.SpecialSales)
                .WithLineItem(i => i
                    .WithQuantity(1)
                    .WithUnitPrice(100m)
                    .WithTaxCategory(TaxCategoryCode.S)
                    .WithSpecialTaxAmount(5m))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(100m)
                    .WithTotalVatAmount(16m)          // 16 % VAT
                    .WithTotalSpecialTaxAmount(5m)    // explicit special tax
                    .WithTotalInvoiceAmount(121m))    // 100 + 16 + 5
                .Build();

            var ubl = InvoiceGeneratorService.GenerateUBL21(invoice);

            Assert.Equal(2, ubl.TaxTotal.Count);          // VAT + SpecialTax
            Assert.Equal(16m, ubl.TaxTotal[0].TaxAmount.Value);
            Assert.Equal(5m, ubl.TaxTotal[1].TaxAmount.Value);
            Assert.Equal(121m, ubl.LegalMonetaryTotal.TaxInclusiveAmount.Value);
        }

        [Fact]
        public void GenerateUBL21_WithDifferentTaxCategories_AppliesCorrectRates()
        {
            var invoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithMultipleLineItems(
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.S),
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.S5),
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.Z))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(300m)
                    .WithTotalVatAmount(21m)          // 16 + 5 + 0
                    .WithTotalSpecialTaxAmount(0m)
                    .WithTotalInvoiceAmount(321m))
                .Build();

            var ubl = InvoiceGeneratorService.GenerateUBL21(invoice);

            Assert.Equal(3, ubl.InvoiceLine.Count);
            Assert.Equal(21m, ubl.TaxTotal[0].TaxAmount.Value);
        }
    }
}