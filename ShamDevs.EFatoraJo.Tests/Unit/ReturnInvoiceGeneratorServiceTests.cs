using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Services;
using ShamDevs.EFatoraJo.Tests.Builders;
using Xunit.Abstractions;

namespace ShamDevs.EFatoraJo.Tests.Unit
{
    public class ReturnInvoiceGeneratorServiceTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        public void GenerateReturnUBL21_WithMinimalValidInvoice_GeneratesValidReturnUBL()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithBasicDetails("INV-001", "2023-01-01", InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithSupplier(s => s.WithTaxVATNumber("JO123456789"))
                .WithCustomer(c => c.WithName("Test Customer"))
                .WithLineItem(i => i.WithQuantity(1).WithUnitPrice(10m).WithTaxCategory(TaxCategoryCode.S))
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                invoiceNumber: "RET-001",
                returnedInvoice: originalInvoice,
                uniqueSerialNumber: Guid.NewGuid().ToString(),
                invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
                returnReason: "Return reason");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.NotNull(ublReturnInvoice);
            Assert.Equal("RET-001", ublReturnInvoice.ID.Value);
            Assert.Equal("381", ublReturnInvoice.InvoiceTypeCode.Value);
            Assert.Equal("Return reason", ublReturnInvoice.PaymentMeans[0].InstructionNote[0].Value);
            Assert.Single(ublReturnInvoice.InvoiceLine);
        }

        [Fact]
        public void GenerateReturnUBL21_WithAllFields_GeneratesCompleteUBL()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
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
                    .WithTaxCategory(TaxCategoryCode.S)) // Standard VAT (16%)
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-002",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Complete return");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.Equal("RET-002", ublReturnInvoice.ID.Value);
            Assert.Equal("Test Supplier",
                ublReturnInvoice.AccountingSupplierParty.Party.PartyLegalEntity[0].RegistrationName.Value);
            Assert.Equal("CUST-123",
                ublReturnInvoice.AccountingCustomerParty.Party.PartyIdentification[0].ID.Value);
            Assert.Equal(31m, ublReturnInvoice.InvoiceLine[0].LineExtensionAmount.Value); // 2 x 15.50
        }

        [Fact]
        public void GenerateReturnUBL21_WithMultipleLineItems_IncludesAllItems()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithBasicDetails("INV-003", "2023-03-01", InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithMultipleLineItems(
                    i => i.WithId("ITEM-001").WithQuantity(1).WithUnitPrice(10m).WithTaxCategory(TaxCategoryCode.S),
                    i => i.WithId("ITEM-002").WithQuantity(2).WithUnitPrice(20m).WithTaxCategory(TaxCategoryCode.S))
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(50m)  // 10 + 40
                    .WithTotalVatAmount(8m)      // 16% of 50
                    .WithTotalInvoiceAmount(58m)) // 50 + 8
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-MULTI",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Multiple items return");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.Equal(2, ublReturnInvoice.InvoiceLine.Count);
            Assert.Equal("ITEM-001", ublReturnInvoice.InvoiceLine[0].ID.Value);
            Assert.Equal("ITEM-002", ublReturnInvoice.InvoiceLine[1].ID.Value);
            Assert.Equal(50m, ublReturnInvoice.LegalMonetaryTotal.TaxExclusiveAmount.Value);
        }

        [Fact]
        public void GenerateReturnUBL21_WithDiscounts_CalculatesCorrectAmounts()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithLineItem(i => i
                    .WithQuantity(2)
                    .WithUnitPrice(10m)
                    .WithDiscountAmount(1m)
                    .WithTaxCategory(TaxCategoryCode.S)) // Standard VAT (16%)
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(20m)  // 2 x 10
                    .WithTotalDiscountAmount(1m)  // Discount applied
                    .WithTotalVatAmount(3.04m)    // 16% of 19
                    .WithTotalInvoiceAmount(22.04m)) // 20 - 1 + 3.04
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-DISC",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Discount return");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.Equal(1m, ublReturnInvoice.AllowanceCharge[0].Amount.Value);
            Assert.Equal(19m, ublReturnInvoice.InvoiceLine[0].LineExtensionAmount.Value);
        }

        [Fact]
        public void GenerateReturnUBL21_WithTax_IncludesTaxAmounts()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithLineItem(i => i
                    .WithQuantity(1)
                    .WithUnitPrice(100m)
                    .WithTaxCategory(TaxCategoryCode.S)) // Standard VAT (16%)
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(100m)
                    .WithTotalVatAmount(16m)
                    .WithTotalInvoiceAmount(116m))
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-TAX",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Tax return");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.Equal(16m, ublReturnInvoice.TaxTotal[0].TaxAmount.Value);
            Assert.Equal(116m, ublReturnInvoice.LegalMonetaryTotal.TaxInclusiveAmount.Value);
        }

        [Fact]
        public void GenerateReturnUBL21_IncludesCorrectBillingReference()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithBasicDetails("INV-REF", "2023-01-01", InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithUniqueSerialNumber("ORIG-UUID-123")
                .WithLineItem(i => i.WithTaxCategory(TaxCategoryCode.S))
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-REF",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Billing reference test");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.Equal("INV-REF", ublReturnInvoice.BillingReference[0].InvoiceDocumentReference.ID.Value);
            Assert.Equal("ORIG-UUID-123", ublReturnInvoice.BillingReference[0].InvoiceDocumentReference.UUID.Value);
        }

        [Fact]
        public void GenerateReturnUBL21_WithEmptyReturnReason_StillGeneratesValidInvoice()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithLineItem(i => i.WithTaxCategory(TaxCategoryCode.S))
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-EMPTY",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.NotNull(ublReturnInvoice);
            Assert.Equal("RET-EMPTY", ublReturnInvoice.ID.Value);
            Assert.Equal("", ublReturnInvoice.PaymentMeans[0].InstructionNote[0].Value);
        }

        [Fact]
        public void GenerateReturnUBL21_WithDifferentTaxCategories_AppliesCorrectRates()
        {
            // Arrange
            var originalInvoice = new InvoiceBuilder()
                .WithInvoicePaymentType(InvoicePaymentTypeCode.LocalGeneralSalesCash)
                .WithMultipleLineItems(
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.S), // 16%
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.S5), // 5%
                    i => i.WithQuantity(1).WithUnitPrice(100m).WithTaxCategory(TaxCategoryCode.Z)) // 0%
                .WithTotals(t => t
                    .WithTotalBeforeDiscount(300m)
                    .WithTotalVatAmount(21m) // (16 + 5 + 0)
                    .WithTotalInvoiceAmount(321m))
                .Build();

            var returnInvoice = new SalesReturnInvoice(
                "RET-MULTITAX",
                originalInvoice,
                Guid.NewGuid().ToString(),
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Multiple tax categories return");

            // Act
            var ublReturnInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(returnInvoice);

            // Assert
            Assert.Equal(3, ublReturnInvoice.InvoiceLine.Count);
            Assert.Equal(21m, ublReturnInvoice.TaxTotal[0].TaxAmount.Value);
        }
    }
}