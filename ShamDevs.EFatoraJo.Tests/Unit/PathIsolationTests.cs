using EFatoraJoConsoleApp.Serialization;
using System.Text.Json;

namespace ShamDevs.EFatoraJo.Tests.Unit
{
    /// <summary>
    /// Tests to verify isolation between regular invoice and return invoice processing paths.
    /// These tests ensure that files intended for one path are rejected by the other.
    /// </summary>
    public class PathIsolationTests
    {
        #region InvoiceJsonParser Isolation Tests

        [Fact]
        public void InvoiceJsonParser_RejectsJsonWithReturnReason_ThrowsJsonException()
        {
            // Arrange - JSON that looks like a return invoice (has returnReason)
            var returnInvoiceJson = """
            {
                "invoiceNumber": "INV-001",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "invoiceDate": "2024-01-15",
                "paymentType": "LocalGeneralSalesCash",
                "returnReason": "Customer return",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => InvoiceJsonParser.ParseInvoice(returnInvoiceJson));
            Assert.Contains("return invoice", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void InvoiceJsonParser_RejectsJsonWithReturnedInvoice_ThrowsJsonException()
        {
            // Arrange - JSON with returnedInvoice property
            var returnInvoiceJson = """
            {
                "invoiceNumber": "INV-001",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "invoiceDate": "2024-01-15",
                "paymentType": "LocalGeneralSalesCash",
                "returnedInvoice": { "invoiceNumber": "ORIG-001" },
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => InvoiceJsonParser.ParseInvoice(returnInvoiceJson));
            Assert.Contains("return invoice", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void InvoiceJsonParser_AcceptsValidRegularInvoice_ReturnsInvoice()
        {
            // Arrange - Valid regular invoice JSON
            var validInvoiceJson = """
            {
                "invoiceNumber": "INV-001",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "invoiceDate": "2024-01-15",
                "paymentType": "LocalGeneralSalesCash",
                "type": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act
            var invoice = InvoiceJsonParser.ParseInvoice(validInvoiceJson);

            // Assert
            Assert.NotNull(invoice);
            Assert.Equal("INV-001", invoice.InvoiceNumber);
            Assert.Equal(100m, invoice.InvoiceTotals.TotalBeforeDiscount);
            Assert.True(invoice.InvoiceTotals.TotalBeforeDiscount > 0, "Regular invoice should have positive amounts");
        }

        #endregion

        #region ReturnInvoiceJsonParser Isolation Tests

        [Fact]
        public void ReturnInvoiceJsonParser_RequiresTypeSalesReturn_ThrowsJsonException()
        {
            // Arrange - JSON without type=SalesReturn
            var invalidReturnJson = """
            {
                "originalInvoiceNumber": "INV-001",
                "returnInvoiceNumber": "RET-001",
                "returnReason": "Customer return",
                "type": "GeneralSales",
                "invoiceDate": "2024-01-15",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "paymentType": "LocalGeneralSalesCash",
                "originalInvoiceType": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => ReturnInvoiceJsonParser.Parse(invalidReturnJson));
            Assert.Contains("SalesReturn", ex.Message);
        }

        [Fact]
        public void ReturnInvoiceJsonParser_RequiresOriginalInvoiceNumber_ThrowsJsonException()
        {
            // Arrange - JSON without originalInvoiceNumber
            var invalidReturnJson = """
            {
                "returnInvoiceNumber": "RET-001",
                "returnReason": "Customer return",
                "type": "SalesReturn",
                "invoiceDate": "2024-01-15",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "paymentType": "LocalGeneralSalesCash",
                "originalInvoiceType": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => ReturnInvoiceJsonParser.Parse(invalidReturnJson));
            Assert.Contains("originalInvoiceNumber", ex.Message);
        }

        [Fact]
        public void ReturnInvoiceJsonParser_AppliesNegativeSigns_ToAmounts()
        {
            // Arrange - Valid return invoice JSON with positive amounts
            var validReturnJson = """
            {
                "originalInvoiceNumber": "INV-001",
                "returnInvoiceNumber": "RET-001",
                "returnReason": "Customer return",
                "type": "SalesReturn",
                "invoiceDate": "2024-01-15",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "paymentType": "LocalGeneralSalesCash",
                "originalInvoiceType": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act
            var result = ReturnInvoiceJsonParser.Parse(validReturnJson);

            // Assert - Amounts should be negative for return invoices
            Assert.NotNull(result);
            Assert.NotNull(result.OriginalInvoice);

            // Totals should be negative
            Assert.True(result.OriginalInvoice.InvoiceTotals.TotalBeforeDiscount < 0,
                "Return invoice TotalBeforeDiscount should be negative");
            Assert.True(result.OriginalInvoice.InvoiceTotals.TotalVATAmount < 0,
                "Return invoice TotalVATAmount should be negative");
            Assert.True(result.OriginalInvoice.InvoiceTotals.TotalInvoiceAmount < 0,
                "Return invoice TotalInvoiceAmount should be negative");
            Assert.True(result.OriginalInvoice.InvoiceTotals.FinalPayableAmount < 0,
                "Return invoice FinalPayableAmount should be negative");

            // Line item amounts should be negative
            var detail = result.OriginalInvoice.InvoiceDetails[0];
            Assert.True(detail.Quantity < 0, "Return invoice Quantity should be negative");
            Assert.True(detail.UnitPriceBeforeTax < 0, "Return invoice UnitPriceBeforeTax should be negative");
            Assert.True(detail.TotalBeforeTax < 0, "Return invoice TotalBeforeTax should be negative");
            Assert.True(detail.TaxAmount < 0, "Return invoice TaxAmount should be negative");
            Assert.True(detail.TotalIncludingTax < 0, "Return invoice TotalIncludingTax should be negative");
        }

        [Fact]
        public void ReturnInvoiceJsonParser_AcceptsValidReturnInvoice_ReturnsInput()
        {
            // Arrange - Complete valid return invoice JSON
            var validReturnJson = """
            {
                "originalInvoiceNumber": "INV-001",
                "returnInvoiceNumber": "RET-001",
                "returnReason": "Customer return - defective product",
                "type": "SalesReturn",
                "invoiceDate": "2024-01-15",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "paymentType": "LocalGeneralSalesCash",
                "originalInvoiceType": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act
            var result = ReturnInvoiceJsonParser.Parse(validReturnJson);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("INV-001", result.OriginalInvoiceNumber);
            Assert.Equal("RET-001", result.ReturnInvoiceNumber);
            Assert.Equal("Customer return - defective product", result.ReturnReason);
            Assert.NotNull(result.OriginalInvoice);
        }

        [Fact]
        public void ReturnInvoiceJsonParser_RejectsRegularInvoiceFile_ThrowsJsonException()
        {
            // Arrange - Regular invoice JSON (has invoiceNumber but not originalInvoiceNumber)
            var regularInvoiceJson = """
            {
                "invoiceNumber": "INV-001",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "invoiceDate": "2024-01-15",
                "paymentType": "LocalGeneralSalesCash",
                "type": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => ReturnInvoiceJsonParser.Parse(regularInvoiceJson));
            Assert.Contains("regular invoice", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ReturnInvoiceJsonParser_RejectsNestedReturnedInvoiceStructure_ThrowsJsonException()
        {
            // Arrange - Old SDK format with nested returnedInvoice
            var nestedStructureJson = """
            {
                "invoiceNumber": "RET-001",
                "returnedInvoice": {
                    "invoiceNumber": "INV-001"
                },
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "invoiceDate": "2024-01-15"
            }
            """;

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => ReturnInvoiceJsonParser.Parse(nestedStructureJson));
            Assert.Contains("returnedInvoice", ex.Message);
            Assert.Contains("not supported", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Defensive Copying Tests

        [Fact]
        public void ReturnInvoiceJsonParser_DefensiveCopying_ParseTwiceYieldsSameResults()
        {
            // Arrange - Parse twice with same structure to verify no side effects
            var returnJson = """
            {
                "originalInvoiceNumber": "INV-001",
                "returnInvoiceNumber": "RET-001",
                "returnReason": "Customer return",
                "type": "SalesReturn",
                "invoiceDate": "2024-01-15",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "paymentType": "LocalGeneralSalesCash",
                "originalInvoiceType": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act - Parse twice
            var result1 = ReturnInvoiceJsonParser.Parse(returnJson);
            var result2 = ReturnInvoiceJsonParser.Parse(returnJson);

            // Assert - Both should have same negative values (defensive copying ensures consistency)
            Assert.Equal(result1.OriginalInvoice.InvoiceTotals.TotalBeforeDiscount,
                         result2.OriginalInvoice.InvoiceTotals.TotalBeforeDiscount);
            Assert.Equal(-100m, result1.OriginalInvoice.InvoiceTotals.TotalBeforeDiscount);
            Assert.Equal(-100m, result2.OriginalInvoice.InvoiceTotals.TotalBeforeDiscount);
        }

        #endregion

        #region Cross-Path Isolation Tests

        [Fact]
        public void RegularInvoiceAmounts_ArePositive_AfterParsing()
        {
            // Arrange
            var validInvoiceJson = """
            {
                "invoiceNumber": "INV-001",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "invoiceDate": "2024-01-15",
                "paymentType": "LocalGeneralSalesCash",
                "type": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act
            var invoice = InvoiceJsonParser.ParseInvoice(validInvoiceJson);

            // Assert - All amounts should remain positive (no sign conversion)
            Assert.True(invoice.InvoiceTotals.TotalBeforeDiscount > 0);
            Assert.True(invoice.InvoiceTotals.TotalVATAmount > 0);
            Assert.True(invoice.InvoiceTotals.TotalInvoiceAmount > 0);
            Assert.True(invoice.InvoiceTotals.FinalPayableAmount > 0);
            Assert.True(invoice.InvoiceDetails[0].Quantity > 0);
            Assert.True(invoice.InvoiceDetails[0].UnitPriceBeforeTax > 0);
        }

        [Fact]
        public void ReturnInvoiceAmounts_AreNegative_AfterParsing()
        {
            // Arrange
            var validReturnJson = """
            {
                "originalInvoiceNumber": "INV-001",
                "returnInvoiceNumber": "RET-001",
                "returnReason": "Customer return",
                "type": "SalesReturn",
                "invoiceDate": "2024-01-15",
                "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
                "paymentType": "LocalGeneralSalesCash",
                "originalInvoiceType": "GeneralSales",
                "supplier": {
                    "taxVATNumber": "123456789",
                    "incomeSourceSequence": "1",
                    "registeredSupplierName": "Test Supplier"
                },
                "customer": { "name": "Test Customer" },
                "invoiceTotals": {
                    "totalVATAmount": 16,
                    "totalSpecialTaxAmount": 0,
                    "totalBeforeDiscount": 100,
                    "totalInvoiceAmount": 116,
                    "totalDiscountAmount": 0,
                    "finalPayableAmount": 116
                },
                "invoiceDetails": [{
                    "id": "1",
                    "taxCategory": "S",
                    "description": "Test Item",
                    "quantity": 1,
                    "unitPriceBeforeTax": 100,
                    "totalBeforeTax": 100,
                    "taxAmount": 16,
                    "totalIncludingTax": 116
                }]
            }
            """;

            // Act
            var result = ReturnInvoiceJsonParser.Parse(validReturnJson);

            // Assert - All amounts should be negative (sign conversion applied)
            Assert.True(result.OriginalInvoice.InvoiceTotals.TotalBeforeDiscount < 0);
            Assert.True(result.OriginalInvoice.InvoiceTotals.TotalVATAmount < 0);
            Assert.True(result.OriginalInvoice.InvoiceTotals.TotalInvoiceAmount < 0);
            Assert.True(result.OriginalInvoice.InvoiceTotals.FinalPayableAmount < 0);
            Assert.True(result.OriginalInvoice.InvoiceDetails[0].Quantity < 0);
            Assert.True(result.OriginalInvoice.InvoiceDetails[0].UnitPriceBeforeTax < 0);
        }

        #endregion
    }
}
