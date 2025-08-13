# JoFatora .NET Client

[![NuGet Version](https://img.shields.io/nuget/v/ShamDevs.EFatoraJo.Sdk.svg)](https://www.nuget.org/packages/ShamDevs.EFatoraJo.Sdk/)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0%2B-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A high-performance .NET SDK for seamless integration with Jordan's **JoFatora** e-invoicing system. It's built with a clean architecture and comprehensive async support to make compliance with local tax regulations fast and hassle-free.

SDK عالي الأداء مخصص للربط السلس مع منصة الفوترة الإلكترونية الأردنية **JoFatora**. تم بناؤه بهيكلية نظيفة ودعم كامل للعمليات غير المتزامنة، لتسهيل الامتثال للمتطلبات الضريبية المحلية وجعل عملية الربط بسيطة وسريعة.

## Table of Contents

* [Features](#features)
* [Installation](#installation)
* [Quick Start](#quick-start)
* [Usage Examples](#usage-examples)
* [Core Components](#core-components)
* [Validation](#validation)
* [Error Handling](#error-handling)
* [Response Handling](#response-handling)
* [Testing](#testing)
* [API Reference](#api-reference)
* [Requirements](#requirements)
* [Contributing](#contributing)
* [License](#license)
* [Support](#support)

---

## Features

* 🚀 **Full UBL 2.1 Compliance** – Generate standards-compliant invoices and credit notes.
* ✅ **Pre-submission Validation** – Comprehensive invoice validation before API calls.
* 📊 **Rich Response Handling** – Detailed submission results with error breakdowns and warnings.
* 🏗️ **Strong Typing** – Type-safe C# models for all entities and responses.
* ⚡ **High Performance** – Fully async operations with configurable timeouts and retry policies.
* 🔧 **Multiple Invoice Types** – Support for standard, income, special sales, and return invoices.
* 🌍 **Multi-Currency Support** – JOD, USD, EUR with proper formatting.
* 🛡️ **Exception Safety** – Structured exception handling with detailed error context.
* 🧪 **Test Data Generation** – Built-in utilities for generating test invoices.
* 🌐 **Broad .NET Compatibility**: Works across .NET Framework 4.6.1+, .NET Core 2.0+, and .NET 5+.

---

## Installation

### Package Manager

```bash
Install-Package ShamDevs.EFatoraJo.Sdk
```

### .NET CLI

```bash
dotnet add package ShamDevs.EFatoraJo.Sdk
```

### PackageReference

```xml
<PackageReference Include="ShamDevs.EFatoraJo.Sdk" Version="1.0.0" />
```

---

## Quick Start

### 1. Submit a Standard Invoice

```csharp
using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Exceptions;

var invoice = new Invoice
{
    InvoiceNumber = "INV-2025-001",
    UniqueSerialNumber = Guid.NewGuid().ToString(),
    InvoiceDate = DateTime.Now.ToString("yyyy-MM-dd"),
    InvoiceTypeCode = "Tax Invoice",
    InvoiceNote = "Standard sales invoice",
    Type = InvoiceType.Standard,
    Currency = CurrencyCode.JOD,
    Supplier = new Supplier
    {
        RegisteredSupplierName = "My Company Ltd",
        TaxVATNumber = "123456789",
        IncomeSourceSequence = "1"
    },
    Customer = new Customer
    {
        Name = "Customer Name",
        IdentificationType = IdentificationType.NationalID,
        IdentificationNumber = "987654321",
        City = CityCode.Amman,
        PostalCode = "11118",
        PhoneNumber = "+962791234567"
    },
    InvoiceDetails = new List<InvoiceDetail>
    {
        new InvoiceDetail
        {
            ID = "1",
            Description = "Product or Service",
            Quantity = 1,
            UnitPriceBeforeTax = 100.000m,
            TotalBeforeTax = 100.000m,
            TaxCategory = TaxCategoryCode.StandardRate,
            TaxAmount = 16.000m,
            TotalIncludingTax = 116.000m
        }
    },
    InvoiceTotals = new InvoiceTotals
    {
        TotalBeforeDiscount = 100.000m,
        TotalDiscountAmount = 0.000m,
        TotalVATAmount = 16.000m,
        TotalInvoiceAmount = 116.000m
    }
};

try
{
    var response = await EFatoraJoSdk.SendFatoraAsync(invoice, "your_client_id", "your_secret_key");
    if (response.IsSuccessfullySubmitted())
    {
        Console.WriteLine($"✅ Invoice submitted successfully!");
        Console.WriteLine($"📱 QR Code: {response.Qr}");
        Console.WriteLine($"🆔 Submission ID: {response.Results?.SubmissionId}");
    }
    else
    {
        Console.WriteLine($"❌ Submission failed: {response.GetFormattedErrors()}");
    }
}
catch (InvoiceValidationException ex)
{
    Console.WriteLine($"❌ Validation failed: {string.Join(", ", ex.ValidationErrors)}");
}
catch (EInvoiceApiException ex)
{
    Console.WriteLine($"❌ API Error: {ex.Message}");
}
```

### 2. Submit a Sales Return Invoice

```csharp
var returnInvoice = new SalesReturnInvoice
{
    InvoiceNumber = "RET-2025-001",
    ReturnReason = "Customer requested refund",
    OriginalInvoiceNumber = "INV-2025-001",
};

var returnResponse = await EFatoraJoSdk.SendReturnFatoraAsync(
    returnInvoice,
    "your_client_id",
    "your_secret_key");

if (returnResponse.IsSuccessfullySubmitted())
{
    Console.WriteLine("✅ Return invoice processed successfully!");
}
```

---

## Usage Examples

### Invoice Types

#### Standard Sales Invoice

```csharp
var standardInvoice = new Invoice
{
    Type = InvoiceType.Standard,
    // VAT is calculated automatically
};
```

#### Income Invoice (No VAT)

```csharp
var incomeInvoice = new Invoice
{
    Type = InvoiceType.Income,
    // No VAT calculations applied
};
```

#### Special Sales Invoice (with Special Tax)

```csharp
var specialInvoice = new Invoice
{
    Type = InvoiceType.SpecialSales,
    InvoiceDetails = new List<InvoiceDetail>
    {
        new InvoiceDetail
        {
            SpecialTaxAmount = 5.000m, // Special tax amount
            // ... other fields
        }
    }
};
```

### Multi-Currency Support

```csharp
// USD Invoice - CurrencyCode is set automatically
var usdInvoice = new Invoice
{
    Currency = CurrencyCode.USD,
    // CurrencyCode property will be automatically set to "USD"
    // Amounts in USD
};

// EUR Invoice - CurrencyCode is set automatically
var eurInvoice = new Invoice
{
    Currency = CurrencyCode.EUR,
    // CurrencyCode property will be automatically set to "EUR"
    // Amounts in EUR
};
```

---

## Core Components

### Models Overview

| Model | Purpose | Required Fields |
|---|---|---|
| `Invoice` | Standard tax invoice | InvoiceNumber, Supplier, Customer, InvoiceDetails, InvoiceTotals |
| `SalesReturnInvoice` | Credit notes / returns | All Invoice fields + ReturnReason |
| `Supplier` | Seller information | RegisteredSupplierName, TaxVATNumber |
| `Customer` | Buyer information | Name, IdentificationType, IdentificationNumber |
| `InvoiceDetail` | Line items | Description, Quantity, UnitPriceBeforeTax |
| `InvoiceTotals` | Invoice totals | TotalBeforeDiscount, TotalInvoiceAmount |

### Service Classes

| Service | Purpose |
|---|---|
| `InvoiceValidator` | Pre-submission validation |
| `InvoiceGeneratorService` | UBL 2.1 document generation |
| `RandomInvoiceGenerator` | Test data generation |
| `InvoiceHelper` | UBL serialization utilities |

---

## Validation

### Manual Validation

```csharp
var validator = new InvoiceValidator();
var validationResult = validator.Validate(invoice);

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"❌ {error.PropertyName}: {error.ErrorMessage}");
    }
}
```

### Common Validation Rules

* **Invoice Number**: Must be unique and non-empty.
* **Dates**: Must be in ISO format (yyyy-MM-dd).
* **Tax Numbers**: Must be in a valid Jordan tax format.
* **Amounts**: Must be positive and properly formatted (3 decimal places).
* **Phone Numbers**: Must include the country code (+962).
* **Currency**: Must match the invoice currency throughout.

---

## Error Handling

### Exception Types

| Exception | Description | Common Causes |
|---|---|---|
| `InvoiceValidationException` | Invoice data validation failed | Missing required fields, invalid formats |
| `UblGenerationException` | UBL document creation failed | Malformed data, XML generation issues |
| `EInvoiceApiException` | API communication error | Network issues, server errors, invalid credentials |
| `EInvoiceSerializationException` | Serialization/deserialization failed | Invalid XML/JSON, encoding issues |

### Comprehensive Error Handling

```csharp
try
{
    var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
    return response;
}
catch (InvoiceValidationException ex)
{
    _logger.LogWarning("Invoice validation failed: {Errors}", 
        string.Join(", ", ex.ValidationErrors));
    // Handle validation errors - usually user input issues
    return BadRequest(ex.ValidationErrors);
}
catch (EInvoiceApiException ex)
{
    _logger.LogError(ex, "JoFatora API error: {StatusCode}", ex.StatusCode);
    // Handle API errors - retry logic or user notification
    if (ex.IsRetryable)
    {
        // Implement retry logic
        return await RetrySubmission(invoice, clientId, secretKey);
    }
    return StatusCode(502, "External service unavailable");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error during invoice submission");
    return StatusCode(500, "Internal server error");
}
```

---

## Response Handling

### Response Status Checking

```csharp
var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);

// Check overall success
if (response.IsSuccessfullySubmitted())
{
    Console.WriteLine("✅ Success");
}

// Check for errors
if (response.HasErrors())
{
    var errors = response.GetFormattedErrors();
    Console.WriteLine($"❌ Errors: {errors}");
}

// Check for warnings
if (response.HasWarnings())
{
    var warnings = response.GetFormattedWarnings(); 
    Console.WriteLine($"⚠️ Warnings: {warnings}");
}

// Get detailed status
Console.WriteLine($"Status: {response.Results?.Status}");
Console.WriteLine($"Processing Status: {response.Results?.ProcessingStatus}");
```

### Response Data Extraction

```csharp
if (response.IsSuccessfullySubmitted())
{
    // QR Code for printing
    var qrCode = response.Qr;
    
    // Submission tracking
    var submissionId = response.Results?.SubmissionId;
    var timestamp = response.Results?.SubmissionTime;
    
    // Tax authority reference
    var taxReference = response.Results?.TaxAuthorityReference;
    
    // Use these for record keeping, printing, etc.
    await SaveInvoiceRecord(invoice.InvoiceNumber, submissionId, qrCode);
}
```

---

## Testing

### Generate Test Data

The SDK includes `RandomInvoiceGenerator` for generating realistic, valid test data for direct testing with the JoFatora API.

```csharp
// Generate random standard invoice for testing
var testInvoice = RandomInvoiceGenerator.GenerateRandomInvoice();

// Generate random return invoice for testing
var testReturn = RandomInvoiceGenerator.GenerateRandomSalesReturnInvoice();
```

### Console Application Example

The SDK includes a dedicated console application that you can use to generate and submit random invoices to the JoFatora test environment. This is a great way to validate your setup and explore different invoice types.

The application works in these simple steps:

1. **Configuration**: It reads your JoFatora API credentials and supplier information from user secrets.
2. **User Input**: It prompts you to select the type of invoice you want to test (e.g., General Sales, Special Sales, or Income) and the payment method.
3. **Generation**: It uses the `RandomInvoiceGenerator` to create a complete, valid invoice based on your selections.
4. **Submission**: It submits the generated invoice to the JoFatora API.
5. **Return Invoice**: For invoice types that support returns, it automatically generates and submits a corresponding return invoice to test the full lifecycle.
6. **Reporting**: It displays a detailed report on the submission status, including the QR code, submission ID, or any errors and warnings that occurred.

You can run this application directly to test your credentials and confirm that your setup is working correctly before integrating the SDK into your main application.

---

## API Reference

For detailed API documentation, including:

* Complete property mappings between C\# classes and UBL 2.1 XML elements.
* Jordan e-Invoicing Arabic labels.
* Response classes and enums.
* Advanced configuration options.

See **[REFERENCE.md](REFERENCE.md)** for the comprehensive developer reference guide.

---

## Requirements

* **JoFatora API credentials** (Client ID and Secret Key).
* **Network access** to JoFatora API endpoints.

### Compatibility

The JoFatora .NET Client SDK is built on **.NET Standard 2.0+**, ensuring broad compatibility across various .NET platforms. This means you can seamlessly integrate it into applications targeting:

* **.NET Framework 4.6.1+**
* **.NET Core 2.0+** (including all subsequent .NET Core versions)
* **All versions of .NET 5+** (e.g., .NET 5, .NET 6, .NET 7, .NET 8, and future versions)

### Dependencies

The SDK automatically includes these dependencies:

* `System.Text.Json` for JSON processing.
* `System.Xml` for UBL document generation.
* `System.Net.Http` for API communication.

---

## 🤝 Contributing

We welcome contributions! Please see our [**Contributing Guidelines**](CONTRIBUTING.md) for details on:

* 🔧 How to set up your development environment
* 📋 Our coding standards and best practices
* 🔄 The pull request process
* 🐛 How to report bugs and request features
* 💬 Where to ask questions

### Reporting Issues

Please report issues on our [GitHub Issues](https://www.google.com/search?q=https://github.com/yourorg/jofatora-dotnet-sdk/issues) page with:

* A clear description of the problem.
* Steps to reproduce.
* Expected vs. actual behavior.
* Sample code (if applicable).
* Environment details (.NET version, OS, etc.).

---

### Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed version history.

---

## License

MIT License - see [LICENSE](LICENSE.md) file for the full license text.

---

## Support

* 📖 **Documentation**: [Full API Reference](REFERENCE.md)
* 🐛 **Issues**: [GitHub Issues](https://github.com/sham-devs/EFatoraJo/issues)
* 💬 **Discussions**: [GitHub Discussions](https://github.com/sham-devs/EFatoraJo/discussions)

---

**Made with ❤️ by [Sham Software Consultancy](https://shamconsultancy.com/) for the Jordanian developer community**
