# EFatoraJo API Documentation

## Overview

This document provides comprehensive API documentation for the EFatoraJo SDK, including all public methods, models, utilities, and their usage patterns.

---

## Core SDK Classes

### EFatoraJoSdk

The main service class for interacting with Jordan's e-invoicing system (Jofotara).

#### Methods

##### SendFatoraAsync

Submits a standard invoice to Jofotara e-invoicing system.

```csharp
public static async Task<EInvoiceResponse> SendFatoraAsync(
    Invoice invoice, 
    string clientId, 
    string secretKey)
```

**Parameters:**
- `invoice` (Invoice): The invoice to be processed and sent
- `clientId` (string): API client ID provided by Jofotara
- `secretKey` (string): API secret key provided by Jofotara

**Returns:**
- `Task<EInvoiceResponse>`: Response containing submission results

**Exceptions:**
- `InvoiceValidationException`: Invoice fails validation checks
- `UblGenerationException`: UBL document generation fails
- `EInvoiceSerializationException`: XML serialization/deserialization fails
- `EInvoiceApiException`: API communication fails
- `EInvoiceException`: Other unexpected errors

**Example:**
```csharp
try
{
    var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
    if (response.IsSuccessfullySubmitted())
    {
        Console.WriteLine($"Invoice submitted: {response.InvoiceNumber}");
    }
}
catch (InvoiceValidationException ex)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", ex.ValidationErrors)}");
}
```

##### SendReturnFatoraAsync

Submits a sales return invoice to Jofotara e-invoicing system.

```csharp
public static async Task<EInvoiceResponse> SendReturnFatoraAsync(
    SalesReturnInvoice invoice, 
    string clientId, 
    string secretKey)
```

**Parameters:**
- `invoice` (SalesReturnInvoice): The return invoice to be processed
- `clientId` (string): API client ID provided by Jofotara
- `secretKey` (string): API secret key provided by Jofotara

**Returns:**
- `Task<EInvoiceResponse>`: Response containing submission results

**Exceptions:**
- `InvoiceValidationException`: Original invoice fails validation
- `UblGenerationException`: Return UBL document generation fails
- `EInvoiceSerializationException`: XML serialization/deserialization fails
- `EInvoiceApiException`: API communication fails
- `EInvoiceException`: Other unexpected errors

**Example:**
```csharp
var returnInvoice = new SalesReturnInvoice(
    invoiceNumber: "RET-2024-001",
    returnedInvoice: originalInvoice,
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    returnReason: "Customer requested refund");

var response = await EFatoraJoSdk.SendReturnFatoraAsync(returnInvoice, clientId, secretKey);
```

---

## Model Classes

### Invoice

Represents a standard tax invoice with all required fields and properties.

#### Constructor

```csharp
public Invoice(
    string invoiceNumber,
    string uniqueSerialNumber,
    string invoiceDate,
    InvoicePaymentTypeCode paymentType,
    Supplier supplier,
    Customer customer,
    InvoiceTotals invoiceTotals,
    List<InvoiceDetail> invoiceDetails,
    InvoiceType type = InvoiceType.GeneralSales)
```

#### Properties

| Property | Type | Description | Required |
|----------|-------|-------------|-----------|
| `InvoiceNumber` | string | Unique invoice number | Yes |
| `UniqueSerialNumber` | string | Unique serial number (UUID preferred) | Yes |
| `InvoiceDate` | string | Invoice date (yyyy-MM-dd) | Yes |
| `PaymentType` | InvoicePaymentTypeCode | Payment method type | Yes |
| `Supplier` | Supplier | Seller information | Yes |
| `Customer` | Customer | Buyer information | Yes |
| `InvoiceTotals` | InvoiceTotals | Invoice totals | Yes |
| `InvoiceDetails` | List<InvoiceDetail> | Line items | Yes |
| `Type` | InvoiceType | Invoice type (Standard, Income, SpecialSales) | No (defaults to GeneralSales) |
| `InvoiceNote` | string? | Optional notes | No |
| `Currency` | CurrencyCode | Currency code (JOD, USD, EUR) | No (defaults to JOD) |

#### Read-only Properties

| Property | Type | Description |
|----------|-------|-------------|
| `CurrencyCode` | string | String representation of currency |
| `InvoiceTypeCode` | string | String representation of payment type |

#### Example

```csharp
var invoice = new Invoice(
    invoiceNumber: "INV-2024-001",
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    paymentType: InvoicePaymentTypeCode.LocalGeneralSalesCash,
    supplier: supplier,
    customer: customer,
    invoiceTotals: totals,
    invoiceDetails: details,
    type: InvoiceType.GeneralSales)
{
    InvoiceNote = "Standard sales invoice",
    Currency = CurrencyCode.JOD
};
```

### SalesReturnInvoice

Represents a credit note or return invoice.

#### Constructor

```csharp
public SalesReturnInvoice(
    string invoiceNumber,
    Invoice returnedInvoice,
    string uniqueSerialNumber,
    string invoiceDate,
    string returnReason)
```

#### Properties

| Property | Type | Description | Required |
|----------|-------|-------------|-----------|
| `InvoiceNumber` | string | Return invoice number | Yes |
| `ReturnedInvoice` | Invoice | Original invoice being returned | Yes |
| `UniqueSerialNumber` | string | Unique serial number | Yes |
| `InvoiceDate` | string | Return date (yyyy-MM-dd) | Yes |
| `ReturnReason` | string | Reason for return | Yes |

#### Example

```csharp
var returnInvoice = new SalesReturnInvoice(
    invoiceNumber: "RET-2024-001",
    returnedInvoice: originalInvoice,
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    returnReason: "Customer requested refund");
```

### Supplier

Contains supplier (seller) information.

#### Constructor

```csharp
public Supplier(
    string taxVATNumber,
    string incomeSourceSequence,
    string registeredSupplierName)
```

#### Properties

| Property | Type | Description | Required |
|----------|-------|-------------|-----------|
| `TaxVATNumber` | string | Tax/VAT registration number | Yes |
| `IncomeSourceSequence` | string | Income source/activity code | Yes |
| `RegisteredSupplierName` | string | Registered supplier name | Yes |

#### Example

```csharp
var supplier = new Supplier(
    taxVATNumber: "123456789",
    incomeSourceSequence: "62010",
    registeredSupplierName: "My Company Ltd");
```

### Customer

Contains customer (buyer) information.

#### Constructor

```csharp
public Customer(string name)
```

#### Properties

| Property | Type | Description | Required |
|----------|-------|-------------|-----------|
| `Name` | string | Customer name | Yes |
| `IdentificationNumber` | string? | ID number (National ID, Tax ID, etc.) | No |
| `IdentificationType` | IdentificationType? | Type of identification | No |
| `PostalCode` | string? | Postal/ZIP code | No |
| `PhoneNumber` | string? | Phone number | No |
| `City` | CityCode? | City code (Jordanian governorates) | No |

#### Example

```csharp
var customer = new Customer("Ahmad Hassan")
{
    IdentificationNumber = "987654321",
    IdentificationType = IdentificationType.NationalID,
    City = CityCode.Amman,
    PostalCode = "11118",
    PhoneNumber = "+962791234567"
};
```

### InvoiceDetail

Represents a single line item in an invoice.

#### Constructor

```csharp
public InvoiceDetail(
    string id,
    TaxCategoryCode taxCategory,
    string description)
```

#### Properties

| Property | Type | Description | Required |
|----------|-------|-------------|-----------|
| `ID` | string | Line item ID | Yes |
| `TaxCategory` | TaxCategoryCode | Tax category code | Yes |
| `Description` | string | Item description | Yes |
| `Quantity` | decimal | Quantity | Yes |
| `UnitPriceBeforeTax` | decimal | Unit price excluding tax | Yes |
| `TotalBeforeTax` | decimal | Line total before tax | Yes |
| `DiscountAmount` | decimal? | Discount amount | No |
| `TaxAmount` | decimal | Tax amount | Yes |
| `TotalIncludingTax` | decimal | Line total including tax | Yes |
| `TaxRate` | decimal | Tax rate (decimal, e.g., 0.16 for 16%) | Yes |
| `SpecialTaxAmount` | decimal? | Special tax amount (for SpecialSales) | No |
| `UnitCode` | string | Unit code (default: "PCE") | No |
| `TaxCategoryId` | string | Tax category ID | Yes |
| `TaxSchemeId` | string | Tax scheme ID | Yes |
| `BaseQuantity` | decimal | Base quantity (default: 1) | No |

#### Example

```csharp
var detail = new InvoiceDetail(
    id: "LINE-1",
    taxCategory: TaxCategoryCode.S,
    description: "Product A")
{
    Quantity = 5,
    UnitPriceBeforeTax = 20.00m,
    TotalBeforeTax = 100.00m,
    TaxAmount = 16.00m,
    TotalIncludingTax = 116.00m,
    TaxRate = 0.16m
};
```

### InvoiceTotals

Contains invoice monetary totals.

#### Properties

| Property | Type | Description | Required |
|----------|-------|-------------|-----------|
| `TotalVATAmount` | decimal | Total VAT amount | Yes |
| `TotalSpecialTaxAmount` | decimal | Total special tax amount | Yes |
| `TotalBeforeDiscount` | decimal | Total before discount | Yes |
| `TotalInvoiceAmount` | decimal | Total invoice amount | Yes |
| `TotalDiscountAmount` | decimal | Total discount amount | Yes |
| `FinalPayableAmount` | decimal | Final payable amount | Yes |

#### Example

```csharp
var totals = new InvoiceTotals
{
    TotalVATAmount = 16.00m,
    TotalSpecialTaxAmount = 0.00m,
    TotalBeforeDiscount = 100.00m,
    TotalInvoiceAmount = 116.00m,
    TotalDiscountAmount = 0.00m,
    FinalPayableAmount = 116.00m
};
```

---

## Utility Classes

### InvoiceValidator

Provides validation functionality for invoices before submission.

#### Methods

##### ValidateInvoice

Validates an invoice according to ZATCA requirements.

```csharp
public static void ValidateInvoice(Invoice? invoice)
```

**Parameters:**
- `invoice` (Invoice?): Invoice to validate

**Exceptions:**
- `InvoiceValidationException`: Contains list of validation errors

**Example:**
```csharp
try
{
    InvoiceValidator.ValidateInvoice(invoice);
    Console.WriteLine("Invoice is valid");
}
catch (InvoiceValidationException ex)
{
    Console.WriteLine($"Validation errors: {string.Join(", ", ex.ValidationErrors)}");
}
```

### RandomInvoiceGenerator

Generates realistic test invoices for development and testing.

#### Methods

##### GenerateRandomInvoice

Generates a random invoice for testing purposes.

```csharp
public static Invoice GenerateRandomInvoice(
    Supplier? supplierInfo = null,
    InvoicePaymentTypeCode? invoicePaymentType = null,
    CurrencyCode? currency = null,
    InvoiceType? invoiceType = null)
```

**Parameters:**
- `supplierInfo` (Supplier?): Optional supplier information
- `invoicePaymentType` (InvoicePaymentTypeCode?): Optional payment type
- `currency` (CurrencyCode?): Optional currency
- `invoiceType` (InvoiceType?): Optional invoice type

**Returns:**
- `Invoice`: Generated random invoice

##### GenerateRandomSalesReturnInvoice

Generates a random return invoice for testing.

```csharp
public static SalesReturnInvoice GenerateRandomSalesReturnInvoice(
    Invoice? originalInvoice = null,
    Supplier? supplierInfo = null,
    CurrencyCode? currency = null)
```

**Parameters:**
- `originalInvoice` (Invoice?): Optional original invoice
- `supplierInfo` (Supplier?): Optional supplier information
- `currency` (CurrencyCode?): Optional currency

**Returns:**
- `SalesReturnInvoice`: Generated random return invoice

**Example:**
```csharp
// Generate test invoice
var testInvoice = RandomInvoiceGenerator.GenerateRandomInvoice(
    invoiceType: InvoiceType.GeneralSales);

// Generate test return invoice
var testReturn = RandomInvoiceGenerator.GenerateRandomSalesReturnInvoice(
    originalInvoice: testInvoice);
```

### CurrencyHelper

Provides currency-specific formatting and rounding operations.

#### Methods

##### Round

Rounds a decimal value according to currency-specific rules.

```csharp
public static decimal Round(decimal value, CurrencyCode currency)
```

**Parameters:**
- `value` (decimal): Value to round
- `currency` (CurrencyCode): Target currency

**Returns:**
- `decimal`: Rounded value

**Example:**
```csharp
decimal rounded = CurrencyHelper.Round(100.1234m, CurrencyCode.JOD);
// Returns: 100.123m (3 decimal places for JOD)
```

---

## Enums

### InvoiceType

Specifies the business meaning of the invoice.

| Value | Description | Use Case |
|--------|-------------|-----------|
| `Standard` | Standard tax invoice | Default sales with VAT |
| `Income` | Income invoice (no VAT) | Services exempt from VAT |
| `GeneralSales` | General sales invoice | Standard taxable sales |
| `SpecialSales` | Special sales invoice | Sales with special tax |

### CurrencyCode

Supported currency codes.

| Value | Description | Decimal Places |
|--------|-------------|----------------|
| `JOD` | Jordanian Dinar | 3 |
| `USD` | US Dollar | 2 |
| `EUR` | Euro | 2 |

### InvoicePaymentTypeCode

Payment method classifications.

| Value | Description |
|--------|-------------|
| `LocalGeneralSalesCash` | Local general sales - cash |
| `LocalGeneralSalesCredit` | Local general sales - credit |
| `LocalIncomeCash` | Local income - cash |
| `LocalIncomeCredit` | Local income - credit |
| `LocalSpecialSalesCash` | Local special sales - cash |
| `LocalSpecialSalesCredit` | Local special sales - credit |

### TaxCategoryCode

VAT tax categories.

| Value | Description | Tax Rate |
|--------|-------------|-----------|
| `S` | Standard rate | 16% |
| `S8` | Reduced rate 8% | 8% |
| `S5` | Reduced rate 5% | 5% |
| `O` | Other services | Varies |
| `Z` | Zero-rated | 0% |

### IdentificationType

Customer identification types.

| Value | Description |
|--------|-------------|
| `NIN` | National ID |
| `PN` | Passport Number |
| `TN` | Tax Number |

### CityCode

Jordanian governorates.

| Value | Description |
|--------|-------------|
| `Amman` | Amman |
| `Irbid` | Irbid |
| `Zarqa` | Zarqa |
| `Balqa` | Balqa |
| `Aqaba` | Aqaba |
| `Jerash` | Jerash |
| `Ajloun` | Ajloun |
| `Madaba` | Madaba |
| `Karak` | Karak |
| `Tafilah` | Tafilah |
| `Maan` | Maan |

---

## Response Classes

### EInvoiceResponse

Root response object from the Jofotara API.

#### Properties

| Property | Type | Description |
|----------|-------|-------------|
| `Results` | EInvoiceResults? | Detailed processing results |
| `Status` | EInvoiceStatus | Overall submission status |
| `SignedInvoice` | string? | Base64-encoded signed XML |
| `Qr` | string? | QR code string |
| `InvoiceNumber` | string? | Jofotara invoice number |
| `InvoiceUuid` | string? | Global UUID |

#### Helper Methods

##### IsSuccessfullySubmitted

Checks if the invoice was successfully submitted.

```csharp
public bool IsSuccessfullySubmitted()
```

##### HasErrors

Checks if the response contains errors.

```csharp
public bool HasErrors()
```

##### HasWarnings

Checks if the response contains warnings.

```csharp
public bool HasWarnings()
```

##### GetFormattedErrors

Returns formatted error messages.

```csharp
public string GetFormattedErrors()
```

##### GetFormattedWarnings

Returns formatted warning messages.

```csharp
public string GetFormattedWarnings()
```

### EInvoiceResults

Detailed processing results.

#### Properties

| Property | Type | Description |
|----------|-------|-------------|
| `Status` | EInvoiceProcessingStatus | Processing status (PASS/ERROR) |
| `Info` | List<EInvoiceMessage>? | Informational messages |
| `Warnings` | List<EInvoiceMessage>? | Warning messages |
| `Errors` | List<EInvoiceMessage>? | Error messages |

### EInvoiceMessage

Single message from the API.

#### Properties

| Property | Type | Description |
|----------|-------|-------------|
| `Type` | EInvoiceMessageType | Message type (INFO/WARNING/ERROR) |
| `Status` | EInvoiceMessageStatus | Message status (PASS/WARNING/ERROR) |
| `Code` | string? | Jofotara internal code |
| `Category` | string? | High-level category |
| `Message` | string? | Human-readable message |

---

## Exception Classes

### InvoiceValidationException

Thrown when invoice data fails validation checks.

#### Properties

| Property | Type | Description |
|----------|-------|-------------|
| `ValidationErrors` | List<string> | List of validation error messages |

#### Constructor

```csharp
public InvoiceValidationException(List<string> validationErrors)
```

### EInvoiceApiException

Thrown when API communication fails.

#### Properties

| Property | Type | Description |
|----------|-------|-------------|
| `StatusCode` | int | HTTP status code |
| `ResponseContent` | string? | Raw response content |
| `IsRetryable` | bool | Whether the error is retryable |

#### Constructor

```csharp
public EInvoiceApiException(int statusCode, string? response = null)
```

### UblGenerationException

Thrown when UBL document generation fails.

### EInvoiceSerializationException

Thrown when XML serialization/deserialization fails.

### EInvoiceException

Base exception for all EFatoraJo exceptions.

---

## Integration Examples

### ASP.NET Core Integration

```csharp
public class InvoiceService
{
    private readonly string _clientId;
    private readonly string _secretKey;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IConfiguration configuration, ILogger<InvoiceService> logger)
    {
        _clientId = configuration["EFatora:ClientId"];
        _secretKey = configuration["EFatora:SecretKey"];
        _logger = logger;
    }

    public async Task<SubmissionResult> SubmitInvoiceAsync(InvoiceDto invoiceDto)
    {
        try
        {
            var invoice = MapToInvoice(invoiceDto);
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, _clientId, _secretKey);
            
            if (response.IsSuccessfullySubmitted())
            {
                _logger.LogInformation("Invoice {InvoiceNumber} submitted successfully", 
                    response.InvoiceNumber);
                return new SubmissionResult { Success = true, InvoiceNumber = response.InvoiceNumber };
            }
            else
            {
                var errors = response.GetFormattedErrors();
                _logger.LogWarning("Invoice submission failed: {Errors}", errors);
                return new SubmissionResult { Success = false, Errors = errors };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit invoice");
            return new SubmissionResult { Success = false, Error = ex.Message };
        }
    }
}
```

### Dependency Injection Setup

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<EFatoraOptions>(configuration.GetSection("EFatora"));
    services.AddTransient<IInvoiceService, InvoiceService>();
}

// appsettings.json
{
  "EFatora": {
    "ClientId": "your-client-id",
    "SecretKey": "your-secret-key"
  }
}
```

---

## Best Practices

### 1. Error Handling

- Always wrap SDK calls in try-catch blocks
- Log detailed error information
- Implement retry logic for retryable errors
- Provide user-friendly error messages

### 2. Performance

- Use async/await patterns
- Implement proper timeout handling
- Cache static reference data
- Use connection pooling for high-volume scenarios

### 3. Security

- Never log sensitive credentials
- Use secure configuration storage
- Validate input data before processing
- Implement audit logging

### 4. Testing

- Use RandomInvoiceGenerator for test data
- Test all invoice types
- Validate error handling paths
- Mock API responses for unit tests

---

## Version History

### Version 1.0.0

- Initial release with core functionality
- Support for all invoice types
- UBL 2.1 compliance
- Comprehensive validation
- Error handling and logging

---

## Support

For API support and questions:
- Documentation: See REFERENCE.md for detailed mappings
- Issues: Report on GitHub repository
- Examples: See README.md for usage examples