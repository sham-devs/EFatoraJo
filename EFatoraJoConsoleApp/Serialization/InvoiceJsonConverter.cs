using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EFatoraJoConsoleApp.Serialization;

/// <summary>
/// Custom JSON converter for Invoice with strict validation
/// </summary>
public class InvoiceJsonConverter : JsonConverter<Invoice>
{
    public override Invoice? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        try
        {
            // Check for common mistakes first
            DetectCommonMistakes(root);

            // Extract all required properties with validation
            var invoiceNumber = GetRequiredString(root, "invoiceNumber");
            var uniqueSerialNumber = GetRequiredString(root, "uniqueSerialNumber");
            var invoiceDate = GetRequiredDate(root, "invoiceDate");
            var paymentType = GetRequiredEnum<InvoicePaymentTypeCode>(root, "paymentType");
            var invoiceType = GetOptionalEnum<InvoiceType>(root, "type") ?? InvoiceType.GeneralSales;
            var currency = GetOptionalEnum<CurrencyCode>(root, "currency") ?? CurrencyCode.JOD;

            // Parse nested objects
            var supplier = ParseSupplier(root);
            var customer = ParseCustomer(root);
            var invoiceTotals = ParseInvoiceTotals(root);
            var invoiceDetails = ParseInvoiceDetails(root);

            // Create invoice
            var invoice = new Invoice(
                invoiceNumber: invoiceNumber,
                uniqueSerialNumber: uniqueSerialNumber,
                invoiceDate: invoiceDate,
                paymentType: paymentType,
                supplier: supplier,
                customer: customer,
                invoiceTotals: invoiceTotals,
                invoiceDetails: invoiceDetails,
                type: invoiceType
            )
            {
                Currency = currency
            };

            // Optional invoice note
            if (root.TryGetProperty("invoiceNote", out var noteElement) && noteElement.ValueKind == JsonValueKind.String)
            {
                invoice.InvoiceNote = noteElement.GetString();
            }

            return invoice;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Failed to deserialize Invoice: {ex.Message}", ex);
        }
    }

    private static void DetectCommonMistakes(JsonElement root)
    {
        // Check if this looks like a return invoice file instead of a regular invoice
        if (root.TryGetProperty("returnReason", out _) || root.TryGetProperty("returnedInvoice", out _))
        {
            throw new JsonException(
                "This file appears to be a return invoice with nested structure. " +
                "Return invoice files should use the regular invoice JSON structure (not nested). " +
                "Remove 'returnReason' and 'returnedInvoice' properties from the JSON file. " +
                "The return invoice structure is created internally by the application when you use --return-file option."
            );
        }
    }

    public override void Write(Utf8JsonWriter writer, Invoice value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Serialization is not supported");
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

    private static string GetRequiredDate(JsonElement element, string propertyName)
    {
        var dateStr = GetRequiredString(element, propertyName);

        // Validate date format yyyy-MM-dd
        if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date))
        {
            throw new JsonException($"Property '{propertyName}' must be in yyyy-MM-dd format. Got: {dateStr}");
        }

        // Check if date is not in the future
        if (date > DateTime.Now.Date)
        {
            throw new JsonException($"Property '{propertyName}' cannot be in the future. Got: {dateStr}");
        }

        return dateStr;
    }

    private static TEnum GetRequiredEnum<TEnum>(JsonElement element, string propertyName) where TEnum : struct, Enum
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            throw new JsonException($"Required property '{propertyName}' is missing");
        }

        return ParseEnum<TEnum>(prop, propertyName);
    }

    private static TEnum? GetOptionalEnum<TEnum>(JsonElement element, string propertyName) where TEnum : struct, Enum
    {
        if (!element.TryGetProperty(propertyName, out var prop) || prop.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return ParseEnum<TEnum>(prop, propertyName);
    }

    private static TEnum ParseEnum<TEnum>(JsonElement element, string propertyName) where TEnum : struct, Enum
    {
        // Try parsing as string first
        if (element.ValueKind == JsonValueKind.String)
        {
            var strValue = element.GetString();
            if (Enum.TryParse<TEnum>(strValue, true, out var enumValue))
            {
                return enumValue;
            }
            throw new JsonException($"Invalid {typeof(TEnum).Name} value '{strValue}' for property '{propertyName}'");
        }

        // Try parsing as number
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

    private static Supplier ParseSupplier(JsonElement root)
    {
        if (!root.TryGetProperty("supplier", out var supplierElement))
        {
            throw new JsonException("Required property 'supplier' is missing");
        }

        var taxVATNumber = GetRequiredString(supplierElement, "taxVATNumber");
        var incomeSourceSequence = GetRequiredString(supplierElement, "incomeSourceSequence");
        var registeredSupplierName = GetRequiredString(supplierElement, "registeredSupplierName");

        return new Supplier(taxVATNumber, incomeSourceSequence, registeredSupplierName);
    }

    private static Customer ParseCustomer(JsonElement root)
    {
        if (!root.TryGetProperty("customer", out var customerElement))
        {
            throw new JsonException("Required property 'customer' is missing");
        }

        var name = GetRequiredString(customerElement, "name");

        var customer = new Customer(name);

        // Optional properties
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

        if (customerElement.TryGetProperty("city", out var city))
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

                // Optional fields
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
}
