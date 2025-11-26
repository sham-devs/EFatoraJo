using EFatoraJoConsoleApp.Models;
using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using System.Globalization;
using System.Text.Json;

namespace EFatoraJoConsoleApp.Serialization;

/// <summary>
/// Parses and validates return invoice input according to the explicit schema.
/// </summary>
public static class ReturnInvoiceJsonParser
{
    private const string SalesReturnLiteral = "SalesReturn";
    private const string PaymentTypeSentinel = "SameAsOriginal";

    /// <summary>
    /// Parse return invoice input JSON into a validated DTO.
    /// </summary>
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

        // Convert amounts to negatives for return semantics
        ApplyReturnSigns(invoice.InvoiceTotals, invoice.InvoiceDetails);

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
    /// Applies negative signs to totals and details for return invoice semantics.
    /// WARNING: This method modifies the objects in-place. The objects should be
    /// freshly created and not shared with other code.
    /// </summary>
    /// <remarks>
    /// Input values are expected to be positive (from original invoice).
    /// The Negate function only negates positive values, leaving zero or
    /// negative values unchanged as a safety measure.
    /// </remarks>
    /// <param name="totals">Invoice totals to negate (modified in-place)</param>
    /// <param name="details">Invoice line items to negate (modified in-place)</param>
    private static void ApplyReturnSigns(InvoiceTotals totals, List<InvoiceDetail> details)
    {
        totals.TotalVATAmount = Negate(totals.TotalVATAmount);
        totals.TotalSpecialTaxAmount = Negate(totals.TotalSpecialTaxAmount);
        totals.TotalBeforeDiscount = Negate(totals.TotalBeforeDiscount);
        totals.TotalInvoiceAmount = Negate(totals.TotalInvoiceAmount);
        totals.TotalDiscountAmount = Negate(totals.TotalDiscountAmount);
        totals.FinalPayableAmount = Negate(totals.FinalPayableAmount);

        foreach (var detail in details)
        {
            detail.Quantity = Negate(detail.Quantity);
            detail.UnitPriceBeforeTax = Negate(detail.UnitPriceBeforeTax);
            detail.TotalBeforeTax = Negate(detail.TotalBeforeTax);
            detail.TaxAmount = Negate(detail.TaxAmount);
            detail.TotalIncludingTax = Negate(detail.TotalIncludingTax);

            if (detail.SpecialTaxAmount.HasValue)
            {
                detail.SpecialTaxAmount = Negate(detail.SpecialTaxAmount.Value);
            }

            if (detail.DiscountAmount.HasValue)
            {
                detail.DiscountAmount = Negate(detail.DiscountAmount.Value);
            }
        }
    }

    private static decimal Negate(decimal value) => value > 0 ? -value : value;
    private static int Negate(int value) => value > 0 ? -value : value;

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
