using EFatoraJoConsoleApp.Models;
using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using System.Globalization;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Serialization;

/// <summary>
/// Parses and validates return invoice JSON input according to the explicit schema.
/// This parser is isolated from <see cref="InvoiceJsonParser"/> and handles only return invoice files.
/// </summary>
/// <remarks>
/// <para>Key differences from regular invoice parsing:</para>
/// <list type="bullet">
///   <item><description>Requires 'originalInvoiceNumber', 'returnInvoiceNumber', and 'returnReason' fields</description></item>
///   <item><description>Requires 'type' to be 'SalesReturn'</description></item>
///   <item><description>Automatically negates all monetary amounts and quantities for return semantics</description></item>
/// </list>
/// <para>This parser uses defensive copying to ensure parsed objects are not shared or modified unexpectedly.</para>
/// </remarks>
public static class ReturnInvoiceJsonParser
{
    private const string SalesReturnLiteral = "SalesReturn";
    private const string PaymentTypeSentinel = "SameAsOriginal";

    /// <summary>
    /// Parse return invoice input JSON into a validated DTO with negated amounts.
    /// </summary>
    /// <param name="json">JSON string containing return invoice data</param>
    /// <returns>Validated <see cref="ReturnInvoiceInput"/> with negated monetary amounts</returns>
    /// <exception cref="JsonException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>JSON is empty or malformed</description></item>
    ///   <item><description>Required fields are missing (originalInvoiceNumber, returnInvoiceNumber, returnReason, type)</description></item>
    ///   <item><description>Type is not 'SalesReturn'</description></item>
    ///   <item><description>A regular invoice file is used instead of return invoice file</description></item>
    /// </list>
    /// </exception>
    public static ReturnInvoiceInput Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new JsonException("JSON input cannot be empty");
        }

        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var root = doc.RootElement;

        // Detect common mistakes early for better error messages
        DetectCommonMistakes(root);

        var originalInvoiceNumber = GetRequiredString(root, "originalInvoiceNumber");
        var returnInvoiceNumber = GetRequiredString(root, "returnInvoiceNumber");
        var returnReason = GetRequiredString(root, "returnReason");
        var typeLiteral = GetRequiredString(root, "type");

        if (!typeLiteral.Equals(SalesReturnLiteral, StringComparison.OrdinalIgnoreCase))
        {
            throw new JsonException("Property 'type' must be 'SalesReturn' for return invoices");
        }

        var returnInvoiceDate = GetRequiredDate(root, "invoiceDate");
        var returnUuid = GetOptionalString(root, "returnUUID");

        // Build the original invoice payload (will be sign-adjusted for return)
        var originalInvoice = BuildOriginalInvoice(root);

        return new ReturnInvoiceInput
        {
            OriginalInvoiceNumber = originalInvoiceNumber,
            ReturnInvoiceNumber = returnInvoiceNumber,
            ReturnReason = returnReason,
            ReturnUUID = returnUuid,
            ReturnInvoiceDate = returnInvoiceDate,
            OriginalInvoice = originalInvoice
        };
    }

    private static Invoice BuildOriginalInvoice(JsonElement root)
    {
        var originalInvoiceNumber = GetRequiredString(root, "originalInvoiceNumber");
        var uniqueSerialNumber = GetRequiredUuid(root, "uniqueSerialNumber");
        var invoiceDate = GetRequiredDate(root, "invoiceDate");
        var paymentType = ResolvePaymentType(root);
        var invoiceType = ResolveInvoiceType(root);

        var supplier = ParseSupplier(root);
        var customer = ParseCustomer(root);
        var invoiceTotals = ParseInvoiceTotals(root);
        var invoiceDetails = ParseInvoiceDetails(root);

        // Create invoice using the original invoice number
        var invoice = new Invoice(
            invoiceNumber: originalInvoiceNumber,
            uniqueSerialNumber: uniqueSerialNumber,
            invoiceDate: invoiceDate,
            paymentType: paymentType,
            supplier: supplier,
            customer: customer,
            invoiceTotals: invoiceTotals,
            invoiceDetails: invoiceDetails,
            type: invoiceType);

        // Convert amounts to negatives for return semantics using defensive copies
        var negatedTotals = CreateNegatedTotals(invoice.InvoiceTotals);
        var negatedDetails = CreateNegatedDetails(invoice.InvoiceDetails);

        // Create new invoice with negated values
        invoice = new Invoice(
            invoiceNumber: invoice.InvoiceNumber,
            uniqueSerialNumber: invoice.UniqueSerialNumber,
            invoiceDate: invoice.InvoiceDate,
            paymentType: invoice.PaymentType,
            supplier: invoice.Supplier,
            customer: invoice.Customer,
            invoiceTotals: negatedTotals,
            invoiceDetails: negatedDetails,
            type: invoice.Type);

        if (root.TryGetProperty("currency", out var currencyElement) && currencyElement.ValueKind == JsonValueKind.String)
        {
            if (Enum.TryParse<CurrencyCode>(currencyElement.GetString(), true, out var currency))
            {
                invoice.Currency = currency;
            }
            else
            {
                throw new JsonException($"Invalid currency value '{currencyElement.GetString()}'");
            }
        }

        if (root.TryGetProperty("invoiceNote", out var noteElement) && noteElement.ValueKind == JsonValueKind.String)
        {
            invoice.InvoiceNote = noteElement.GetString();
        }

        return invoice;
    }

    /// <summary>
    /// Creates a new InvoiceTotals with negated values for return invoice semantics.
    /// Uses defensive copying to ensure the original object is not modified.
    /// </summary>
    /// <param name="totals">Original invoice totals (not modified)</param>
    /// <returns>New InvoiceTotals with negated values</returns>
    private static InvoiceTotals CreateNegatedTotals(InvoiceTotals totals)
    {
        return new InvoiceTotals
        {
            TotalVATAmount = Negate(totals.TotalVATAmount),
            TotalSpecialTaxAmount = Negate(totals.TotalSpecialTaxAmount),
            TotalBeforeDiscount = Negate(totals.TotalBeforeDiscount),
            TotalInvoiceAmount = Negate(totals.TotalInvoiceAmount),
            TotalDiscountAmount = Negate(totals.TotalDiscountAmount),
            FinalPayableAmount = Negate(totals.FinalPayableAmount)
        };
    }

    /// <summary>
    /// Creates a new list of InvoiceDetails with negated values for return invoice semantics.
    /// Uses defensive copying to ensure the original objects are not modified.
    /// </summary>
    /// <param name="details">Original invoice details (not modified)</param>
    /// <returns>New list of InvoiceDetails with negated values</returns>
    private static List<InvoiceDetail> CreateNegatedDetails(List<InvoiceDetail> details)
    {
        var negatedDetails = new List<InvoiceDetail>(details.Count);

        foreach (var detail in details)
        {
            var negatedDetail = new InvoiceDetail(detail.ID, detail.TaxCategory, detail.Description)
            {
                Quantity = Negate(detail.Quantity),
                UnitPriceBeforeTax = Negate(detail.UnitPriceBeforeTax),
                TotalBeforeTax = Negate(detail.TotalBeforeTax),
                TaxAmount = Negate(detail.TaxAmount),
                TotalIncludingTax = Negate(detail.TotalIncludingTax),
                SpecialTaxAmount = detail.SpecialTaxAmount.HasValue ? Negate(detail.SpecialTaxAmount.Value) : null,
                DiscountAmount = detail.DiscountAmount.HasValue ? Negate(detail.DiscountAmount.Value) : null
            };

            negatedDetails.Add(negatedDetail);
        }

        return negatedDetails;
    }

    private static decimal Negate(decimal value) => value > 0 ? -value : value;
    private static int Negate(int value) => value > 0 ? -value : value;

    /// <summary>
    /// Detects common mistakes in return invoice JSON structure and provides helpful error messages.
    /// </summary>
    private static void DetectCommonMistakes(JsonElement root)
    {
        // Case 1: Has nested returnedInvoice structure (old SDK format) - check first as it's most specific
        if (root.TryGetProperty("returnedInvoice", out _))
        {
            throw new JsonException(
                "This file uses the old nested 'returnedInvoice' structure which is not supported. " +
                "Return invoice files should use a flat structure with 'originalInvoiceNumber', " +
                "'returnInvoiceNumber', 'returnReason', and invoice data at the root level. " +
                "See sample with: --sample return");
        }

        // Check if this looks like a regular invoice file being used as return invoice
        // Regular invoices have invoiceNumber but not originalInvoiceNumber
        bool hasInvoiceNumber = root.TryGetProperty("invoiceNumber", out _);
        bool hasOriginalInvoiceNumber = root.TryGetProperty("originalInvoiceNumber", out _);
        bool hasReturnReason = root.TryGetProperty("returnReason", out _);
        bool hasType = root.TryGetProperty("type", out var typeElement);

        // Case 2: Has invoiceNumber but missing return-specific fields
        if (hasInvoiceNumber && !hasOriginalInvoiceNumber && !hasReturnReason)
        {
            throw new JsonException(
                "This file appears to be a regular invoice, not a return invoice. " +
                "Return invoice files require 'originalInvoiceNumber', 'returnInvoiceNumber', " +
                "'returnReason', and 'type' set to 'SalesReturn'. " +
                "Use --invoice-file for regular invoices or convert to return invoice format.");
        }

        // Case 3: Has type but it's not SalesReturn (provide clearer message)
        if (hasType && typeElement.ValueKind == JsonValueKind.String)
        {
            var typeValue = typeElement.GetString();
            if (!string.IsNullOrEmpty(typeValue) &&
                !typeValue.Equals(SalesReturnLiteral, StringComparison.OrdinalIgnoreCase) &&
                !hasReturnReason)
            {
                throw new JsonException(
                    $"This file has 'type' set to '{typeValue}' but appears to be used as a return invoice. " +
                    "Return invoices must have 'type' set to 'SalesReturn'. " +
                    "If this is a regular invoice, use --invoice-file instead of --return-file.");
            }
        }
    }

    private static InvoicePaymentTypeCode ResolvePaymentType(JsonElement root)
    {
        if (!root.TryGetProperty("paymentType", out var paymentTypeElement))
        {
            throw new JsonException("Required property 'paymentType' is missing");
        }

        if (paymentTypeElement.ValueKind == JsonValueKind.String &&
            paymentTypeElement.GetString()!.Equals(PaymentTypeSentinel, StringComparison.OrdinalIgnoreCase))
        {
            if (!root.TryGetProperty("originalPaymentType", out var originalPaymentTypeElement))
            {
                throw new JsonException("When 'paymentType' is 'SameAsOriginal', 'originalPaymentType' is required");
            }

            return ParseEnum<InvoicePaymentTypeCode>(originalPaymentTypeElement, "originalPaymentType");
        }

        return ParseEnum<InvoicePaymentTypeCode>(paymentTypeElement, "paymentType");
    }

    private static InvoiceType ResolveInvoiceType(JsonElement root)
    {
        string? originalTypeValue = null;

        if (root.TryGetProperty("originalInvoiceType", out var originalTypeElement))
        {
            originalTypeValue = originalTypeElement.GetString();
        }
        else if (root.TryGetProperty("invoiceType", out var invoiceTypeElement))
        {
            var candidate = invoiceTypeElement.GetString();
            if (!string.IsNullOrWhiteSpace(candidate) &&
                !candidate.Equals(SalesReturnLiteral, StringComparison.OrdinalIgnoreCase))
            {
                originalTypeValue = candidate;
            }
        }

        if (string.IsNullOrWhiteSpace(originalTypeValue))
        {
            throw new JsonException("Property 'originalInvoiceType' (or 'invoiceType') is required for return invoices");
        }

        return ParseEnum<InvoiceType>(originalTypeValue, "originalInvoiceType");
    }

    private static Supplier ParseSupplier(JsonElement root)
    {
        if (!root.TryGetProperty("supplier", out var supplierElement))
        {
            throw new JsonException("Required property 'supplier' is missing");
        }

        var taxVATNumber = GetRequiredString(supplierElement, "taxVATNumber");
        var incomeSourceSequence = GetRequiredString(supplierElement, "incomeSourceSequence");
        var registeredSupplierName = GetRequiredString(supplierElement, "registeredSupplierName");

        var supplier = new Supplier(taxVATNumber, incomeSourceSequence, registeredSupplierName);

        return supplier;
    }

    private static Customer ParseCustomer(JsonElement root)
    {
        if (!root.TryGetProperty("customer", out var customerElement))
        {
            throw new JsonException("Required property 'customer' is missing");
        }

        var name = GetRequiredString(customerElement, "name");

        var customer = new Customer(name);

        if (customerElement.TryGetProperty("identificationNumber", out var idNum) && idNum.ValueKind == JsonValueKind.String)
        {
            customer.IdentificationNumber = idNum.GetString();
        }

        if (customerElement.TryGetProperty("identificationType", out var idType) && idType.ValueKind != JsonValueKind.Null)
        {
            customer.IdentificationType = ParseEnum<IdentificationType>(idType, "identificationType");
        }

        if (customerElement.TryGetProperty("postalCode", out var postal) && postal.ValueKind == JsonValueKind.String)
        {
            customer.PostalCode = postal.GetString();
        }

        if (customerElement.TryGetProperty("phoneNumber", out var phone) && phone.ValueKind == JsonValueKind.String)
        {
            customer.PhoneNumber = phone.GetString();
        }

        if (customerElement.TryGetProperty("city", out var city) && city.ValueKind != JsonValueKind.Null)
        {
            customer.City = ParseEnum<CountrySubentityCode>(city, "city");
        }

        return customer;
    }

    private static InvoiceTotals ParseInvoiceTotals(JsonElement root)
    {
        if (!root.TryGetProperty("invoiceTotals", out var totalsElement))
        {
            throw new JsonException("Required property 'invoiceTotals' is missing");
        }

        return new InvoiceTotals
        {
            TotalVATAmount = GetRequiredDecimal(totalsElement, "totalVATAmount"),
            TotalSpecialTaxAmount = GetRequiredDecimal(totalsElement, "totalSpecialTaxAmount"),
            TotalBeforeDiscount = GetRequiredDecimal(totalsElement, "totalBeforeDiscount"),
            TotalInvoiceAmount = GetRequiredDecimal(totalsElement, "totalInvoiceAmount"),
            TotalDiscountAmount = GetRequiredDecimal(totalsElement, "totalDiscountAmount"),
            FinalPayableAmount = GetRequiredDecimal(totalsElement, "finalPayableAmount")
        };
    }

    private static List<InvoiceDetail> ParseInvoiceDetails(JsonElement root)
    {
        if (!root.TryGetProperty("invoiceDetails", out var detailsElement))
        {
            throw new JsonException("Required property 'invoiceDetails' is missing");
        }

        if (detailsElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Property 'invoiceDetails' must be an array");
        }

        var details = new List<InvoiceDetail>();
        int index = 0;

        foreach (var itemElement in detailsElement.EnumerateArray())
        {
            try
            {
                var id = GetRequiredString(itemElement, "id");
                var taxCategory = GetRequiredEnum<TaxCategoryCode>(itemElement, "taxCategory");
                var description = GetRequiredString(itemElement, "description");

                var detail = new InvoiceDetail(id, taxCategory, description)
                {
                    Quantity = GetRequiredInt(itemElement, "quantity"),
                    UnitPriceBeforeTax = GetRequiredDecimal(itemElement, "unitPriceBeforeTax"),
                    TotalBeforeTax = GetRequiredDecimal(itemElement, "totalBeforeTax"),
                    TaxAmount = GetRequiredDecimal(itemElement, "taxAmount"),
                    TotalIncludingTax = GetRequiredDecimal(itemElement, "totalIncludingTax")
                };

                if (itemElement.TryGetProperty("discountAmount", out var discount) && discount.ValueKind == JsonValueKind.Number)
                {
                    detail.DiscountAmount = discount.GetDecimal();
                }

                if (itemElement.TryGetProperty("specialTaxAmount", out var specialTax) && specialTax.ValueKind == JsonValueKind.Number)
                {
                    detail.SpecialTaxAmount = specialTax.GetDecimal();
                }

                details.Add(detail);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error in invoiceDetails[{index}]: {ex.Message}", ex);
            }

            index++;
        }

        if (details.Count == 0)
        {
            throw new JsonException("Property 'invoiceDetails' must contain at least one item");
        }

        return details;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            throw new JsonException($"Required property '{propertyName}' is missing");
        }

        if (prop.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Property '{propertyName}' must be a string");
        }

        var value = prop.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException($"Property '{propertyName}' cannot be empty");
        }

        return value;
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            return null;
        }

        if (prop.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (prop.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Property '{propertyName}' must be a string");
        }

        var value = prop.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Gets a required UUID/GUID property with format validation
    /// </summary>
    private static string GetRequiredUuid(JsonElement element, string propertyName)
    {
        var value = GetRequiredString(element, propertyName);
        if (!Guid.TryParse(value, out _))
        {
            throw new JsonException(
                $"Property '{propertyName}' must be a valid UUID/GUID format. Got: {value}");
        }
        return value;
    }

    private static string GetRequiredDate(JsonElement element, string propertyName)
    {
        var dateStr = GetRequiredString(element, propertyName);

        if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date))
        {
            throw new JsonException($"Property '{propertyName}' must be in yyyy-MM-dd format. Got: {dateStr}");
        }

        if (date > DateTime.Now.Date)
        {
            throw new JsonException($"Property '{propertyName}' cannot be in the future. Got: {dateStr}");
        }

        return dateStr;
    }

    private static decimal GetRequiredDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            throw new JsonException($"Required property '{propertyName}' is missing");
        }

        if (prop.ValueKind != JsonValueKind.Number)
        {
            throw new JsonException($"Property '{propertyName}' must be a number");
        }

        return prop.GetDecimal();
    }

    private static int GetRequiredInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            throw new JsonException($"Required property '{propertyName}' is missing");
        }

        if (prop.ValueKind != JsonValueKind.Number)
        {
            throw new JsonException($"Property '{propertyName}' must be a number");
        }

        var value = prop.GetInt32();
        if (value <= 0 && propertyName == "quantity")
        {
            throw new JsonException($"Property '{propertyName}' must be greater than 0");
        }

        return value;
    }

    private static TEnum GetRequiredEnum<TEnum>(JsonElement element, string propertyName) where TEnum : struct, Enum
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            throw new JsonException($"Required property '{propertyName}' is missing");
        }

        return ParseEnum<TEnum>(prop, propertyName);
    }

    private static TEnum ParseEnum<TEnum>(JsonElement element, string propertyName) where TEnum : struct, Enum
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var strValue = element.GetString();
            if (Enum.TryParse<TEnum>(strValue, true, out var enumValue))
            {
                return enumValue;
            }
            throw new JsonException($"Invalid {typeof(TEnum).Name} value '{strValue}' for property '{propertyName}'");
        }

        if (element.ValueKind == JsonValueKind.Number)
        {
            var numValue = element.GetInt32();
            if (Enum.IsDefined(typeof(TEnum), numValue))
            {
                return (TEnum)(object)numValue;
            }
            throw new JsonException($"Invalid {typeof(TEnum).Name} numeric value {numValue} for property '{propertyName}'");
        }

        throw new JsonException($"Property '{propertyName}' must be a string or number");
    }

    private static TEnum ParseEnum<TEnum>(string rawValue, string propertyName) where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(rawValue, true, out var enumValue))
        {
            return enumValue;
        }

        throw new JsonException($"Invalid {typeof(TEnum).Name} value '{rawValue}' for property '{propertyName}'");
    }
}
