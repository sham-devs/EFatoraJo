# JoFatora .NET Client

[![NuGet Version](https://img.shields.io/nuget/v/E.FatoraJo.svg?style=flat-square)](https://www.nuget.org/packages/E.FatoraJo/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A high-performance .NET SDK designed for smooth integration with Jordan’s **JoFatora** e-invoicing system.  
Built with clean architecture, fully async support, and simplified compliance with local tax regulations — making integration fast and hassle-free.

SDK عالي الأداء مخصص للربط السلس مع منصة الفوترة الإلكترونية الأردنية **JoFatora**  
تم بناؤه بهيكلية نظيفة ودعم كامل للعمليات غير المتزامنة، لتسهيل الالتزام بالتشريعات الضريبية المحلية وجعل عملية الربط بسيطة وسريعة.

---

## Features

- 🚀 **Full UBL 2.1 Support** – Generate compliant invoices and credit notes  
- 🔒 **Secure Communication** – Built-in authentication with JoFatora API  
- ✅ **Automatic Validation** – Pre-submission invoice validation  
- 📊 **Comprehensive Responses** – Detailed submission results and error breakdown  
- 🏗 **Strong Typing** – Clean C# models for all entities  

---

## Installation

```bash
dotnet add package E.FatoraJo
```

Or via Package Manager:

```bash
Install-Package E.FatoraJo
```

---

## Quick Start

### 1. Submit a Standard Invoice

```csharp
using E_FatoraJo;

var invoice = new Invoice
{
    InvoiceNumber = "INV-2023-001",
    // ... populate other required fields
};

try
{
    var response = await EFatoraJoSdk.SendFatoraAsync(
        invoice,
        "your_client_id",
        "your_secret_key");
    
    if (response.IsSuccessfullySubmitted())
    {
        Console.WriteLine($"Invoice submitted! QR: {response.Qr}");
    }
}
catch (InvoiceValidationException ex)
{
    Console.WriteLine($"Validation errors: {string.Join(", ", ex.ValidationErrors)}");
}
```

### 2. Submit a Return Invoice

```csharp
var returnInvoice = new SalesReturnInvoice
{
    ReturnReason = "Customer return",
    // ... other required fields
};

var returnResponse = await EFatoraJoSdk.SendReturnFatoraAsync(
    returnInvoice,
    "your_client_id",
    "your_secret_key");
```

---

## Core Components

### Models

- `Invoice` – Standard tax invoice
- `SalesReturnInvoice` – Credit notes / returns
- `Supplier` – Seller information
- `Customer` – Buyer information
- `InvoiceDetail` – Line items

### Utilities

- `InvoiceValidator` – Validation engine
- `RandomInvoiceGenerator` – Test data generator
- `InvoiceHelper` – UBL serialization helper

---

## Advanced Usage

### Handling Responses

```csharp
if (response.HasErrors())
{
    var errors = response.GetFormattedErrors();
    var warnings = response.GetFormattedWarnings();
    // Process errors or display messages
}
```

### Generating Test Data

```csharp
var testInvoice = RandomInvoiceGenerator.GenerateRandomInvoice();
var testReturn = RandomInvoiceGenerator.GenerateRandomSalesReturnInvoice();
```

---

## Error Handling

The library throws specific exceptions to simplify debugging:

| Exception                    | Thrown When                                |
|-----------------------------|--------------------------------------------|
| `InvoiceValidationException`| Invoice data fails validation               |
| `UblGenerationException`    | UBL document creation fails                 |
| `EInvoiceApiException`      | Errors during communication with the API   |
| `EInvoiceSerializationException` | Issues during XML/JSON processing     |

---

## Requirements

- .NET Standard 2.0+  
- JoFatora API credentials

---

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you’d like to change.

---

## License

MIT — See [LICENSE](LICENSE) for full license text.
