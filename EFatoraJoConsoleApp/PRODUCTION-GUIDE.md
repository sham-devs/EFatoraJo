# Production Guide - EFatoraJo Console Application

## Overview

A command-line application for submitting electronic invoices to the Jordanian e-invoicing system (Jofotara). The application supports sending invoices from JSON files or direct input with strict validation and machine-readable output.

**Version:** 1.0.0
**Platform:** .NET 8.0
**License:** Per project agreement

---

## Requirements

### System Requirements

- **.NET 8.0 Runtime** (required to run the application)
- **Operating Systems:**
  - Windows 10+ / Server 2016+ (x64)
  - Linux: Ubuntu 18.04+, CentOS 7+, Debian 9+ (x64)
  - macOS 10.15+ Catalina or later (x64)
- **Memory:** 256 MB minimum
- **Storage:** 100 MB

### Operational Requirements

- Active Jofotara account
- **Client ID** and **Secret Key** from Jofotara
- Supplier information:
  - Tax VAT Number
  - Income Source Sequence
  - Registered Supplier Name

---

## Installation and Setup

### 1. Build the Application

```bash
# Build in Release mode
dotnet build -c Release

# The output will be in:
# EFatoraJoConsoleApp\bin\Release\net8.0\
```

### 2. Running the Application

The application requires .NET 8.0 Runtime to be installed on your system.

#### Windows

```powershell
# Navigate to the release directory
cd EFatoraJoConsoleApp\bin\Release\net8.0

# Run using dotnet command
dotnet EFatoraJoConsoleApp.dll --version

# Or if you have the .exe file (framework-dependent)
.\EFatoraJoConsoleApp.exe --version
```

#### Linux

```bash
# Navigate to the release directory
cd EFatoraJoConsoleApp/bin/Release/net8.0

# Run using dotnet command
dotnet EFatoraJoConsoleApp.dll --version
```

#### macOS

```bash
# Navigate to the release directory
cd EFatoraJoConsoleApp/bin/Release/net8.0

# Run using dotnet command
dotnet EFatoraJoConsoleApp.dll --version
```

### 3. Installing .NET 8.0 Runtime (if not installed)

#### Windows

Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

Or using winget:
```powershell
winget install Microsoft.DotNet.Runtime.8
```

#### Linux (Ubuntu/Debian)

```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
```

#### macOS

```bash
# Using Homebrew
brew install --cask dotnet-sdk
```

### 3. Configure Credentials

**⚠️ Important:** Never store credentials in files or Git. Use environment variables or secure credential management.

#### Option A: Using .NET User Secrets (Recommended for Development)

**Prerequisites:** Requires .NET SDK installed

```bash
# Navigate to application directory
cd /path/to/EFatoraJo/

# Configure credentials
dotnet user-secrets set "ClientId" "YOUR_CLIENT_ID_HERE"
dotnet user-secrets set "SecretKey" "YOUR_SECRET_KEY_HERE"

# Configure supplier information
dotnet user-secrets set "Supplier:TaxVATNumber" "1234567890"
dotnet user-secrets set "Supplier:IncomeSourceSequence" "62010"
dotnet user-secrets set "Supplier:RegisteredSupplierName" "Your Company Name"
```

#### Option B: Using Environment Variables (Production Only)

**Note:** The application currently only supports .NET User Secrets for configuration. Environment variables are not automatically loaded by the application.

**Windows (PowerShell):**

```powershell
# For future implementation - currently not supported
# $env:ClientId = "YOUR_CLIENT_ID_HERE"
# $env:SecretKey = "YOUR_SECRET_KEY_HERE"
# $env:Supplier_TaxVATNumber = "1234567890"
# $env:Supplier_IncomeSourceSequence = "62010"
# $env:Supplier_RegisteredSupplierName = "Your Company Name"
```

**Linux/macOS:**

```bash
# For future implementation - currently not supported
# export ClientId="YOUR_CLIENT_ID_HERE"
# export SecretKey="YOUR_SECRET_KEY_HERE"
# export Supplier_TaxVATNumber="1234567890"
# export Supplier_IncomeSourceSequence="62010"
# export Supplier_RegisteredSupplierName="Your Company Name"
```

### 4. Verify Setup

**Windows:**

```powershell
# Test the application
.\EFatoraJoConsoleApp.exe --version

# Display help
.\EFatoraJoConsoleApp.exe --help

# View sample invoice
.\EFatoraJoConsoleApp.exe --sample income
```

**Linux/macOS:**

```bash
# Test the application
./EFatoraJoConsoleApp --version

# Display help
./EFatoraJoConsoleApp --help

# View sample invoice
./EFatoraJoConsoleApp --sample income
```

---

## Interactive Mode

When you run the application without any command-line arguments, it enters interactive mode. This mode is useful for testing and experimenting with the e-invoicing system without creating JSON files.

### Starting Interactive Mode

**Windows:**

```powershell
# Using dotnet
dotnet EFatoraJoConsoleApp.dll

# Or using .exe
.\EFatoraJoConsoleApp.exe
```

**Linux/macOS:**

```bash
dotnet EFatoraJoConsoleApp.dll
```

### Interactive Mode Features

1. **Invoice Type Selection**: Choose from Income, General Sales, or Special Sales invoices
2. **Automatic Invoice Generation**: Creates sample invoices with random data for testing
3. **Direct Submission**: Submits invoices directly to the e-invoicing system
4. **Return Invoice Support**: Optionally creates and submits return invoices
5. **Real-time Feedback**: Shows success/failure messages with detailed error information

### Interactive Mode Workflow

1. Run the application without arguments
2. Select invoice type (1-3):
   - 1: Income Invoice (فاتورة دخل)
   - 2: General Sales Invoice (فاتورة مبيعات عامة)
   - 3: Special Sales Invoice (فاتورة مبيعات خاصة)
3. Application generates a random invoice with test data
4. Review the generated invoice details
5. Application submits the invoice automatically
6. Optionally create a return invoice (y/n)
7. Repeat or exit (0)

### Interactive Mode Configuration

The interactive mode uses the same configuration as command-line mode (User Secrets). Make sure your credentials are properly configured before using interactive mode.

---

## Usage Methods

### Basic Usage

**Using dotnet command (All Platforms):**

```bash
dotnet EFatoraJoConsoleApp.dll [options]
```

**Using executable on Windows:**

```powershell
.\EFatoraJoConsoleApp.exe [options]
```

### Available Options

| Option | Description | Example |
|--------|-------------|---------|
| `--invoice-json <json>` | Invoice JSON as string (use '-' for stdin) | `--invoice-json '{...}'` |
| `--invoice-file <path>` | Path to invoice JSON file | `--invoice-file invoice.json` |
| `--return-json <json>` | Return invoice JSON as string | `--return-json '{...}'` |
| `--return-file <path>` | Path to return invoice JSON file | `--return-file return.json` |
| `--output-format <format>` | Output format: `json` or `text` (default: text) | `--output-format json` |
| `--sample <type>` | Display sample JSON: income, general, special, return | `--sample income` |
| `--help`, `-h`, `-?` | Display help | `--help` |
| `--version`, `-v` | Display version | `--version` |

---

## Usage Examples

### 1. Submit Invoice from File

**Windows:**

```powershell
# Using dotnet
dotnet EFatoraJoConsoleApp.dll --invoice-file C:\data\invoices\invoice.json

# Or using .exe
.\EFatoraJoConsoleApp.exe --invoice-file C:\data\invoices\invoice.json
```

**Linux/macOS:**

```bash
dotnet EFatoraJoConsoleApp.dll --invoice-file /data/invoices/invoice.json
```

### 2. Submit Invoice with JSON Output

**Windows:**

```powershell
# Using dotnet
dotnet EFatoraJoConsoleApp.dll --invoice-file invoice.json --output-format json > result.json

# Or using .exe
.\EFatoraJoConsoleApp.exe --invoice-file invoice.json --output-format json > result.json
```

**Linux/macOS:**

```bash
dotnet EFatoraJoConsoleApp.dll --invoice-file invoice.json --output-format json > result.json
```

### 3. Submit Invoice with Return Invoice

**Note:** The application will automatically handle cases where the original invoice was already submitted. If you submit an already-submitted invoice with a return invoice, the application will skip re-submitting the original and proceed directly to submitting the return invoice.

**Windows:**

```powershell
# Using dotnet
dotnet EFatoraJoConsoleApp.dll `
  --invoice-file invoice.json `
  --return-file return.json `
  --output-format json

# Or using .exe
.\EFatoraJoConsoleApp.exe `
  --invoice-file invoice.json `
  --return-file return.json `
  --output-format json
```

**Linux/macOS:**

```bash
dotnet EFatoraJoConsoleApp.dll \
  --invoice-file invoice.json \
  --return-file return.json \
  --output-format json
```

### 4. Read from Stdin (for piping)

**Windows:**

```powershell
# Using dotnet
Get-Content invoice.json | dotnet EFatoraJoConsoleApp.dll --invoice-json - --output-format json

# Or using .exe
Get-Content invoice.json | .\EFatoraJoConsoleApp.exe --invoice-json - --output-format json
```

**Linux/macOS:**

```bash
cat invoice.json | dotnet EFatoraJoConsoleApp.dll --invoice-json - --output-format json
```

### 5. Display Sample JSON

**Windows:**

```powershell
# Using dotnet
dotnet EFatoraJoConsoleApp.dll --sample income

# Save sample to file
dotnet EFatoraJoConsoleApp.dll --sample general > sample.json

# Or using .exe
.\EFatoraJoConsoleApp.exe --sample income
```

**Linux/macOS:**

```bash
# Display income invoice sample
dotnet EFatoraJoConsoleApp.dll --sample income

# Save sample to file
dotnet EFatoraJoConsoleApp.dll --sample general > sample.json
```

---

## Required JSON Format

### Important Note
The application requires a complete JSON structure that includes all necessary objects and fields. The simplified structure shown in the sample commands (`--sample`) is for demonstration purposes only and will be automatically expanded with required supplier information from your configuration.

### Complete Required JSON Structure

```json
{
  "invoiceNumber": "INV-2024-001",
  "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
  "invoiceDate": "2024-01-15",
  "paymentType": "LocalIncomeCash",
  "type": "Income",
  "currency": "JOD",
  "supplier": {
    "taxVATNumber": "1234567890",
    "incomeSourceSequence": "62010",
    "registeredSupplierName": "Your Company Name"
  },
  "customer": {
    "name": "Customer Name"
  },
  "invoiceTotals": {
    "totalVATAmount": 0.0,
    "totalSpecialTaxAmount": 0.0,
    "totalBeforeDiscount": 1000.0,
    "totalInvoiceAmount": 1000.0,
    "totalDiscountAmount": 0.0,
    "finalPayableAmount": 1000.0
  },
  "invoiceDetails": [
    {
      "id": "LINE-1",
      "taxCategory": "Z",
      "description": "Item Description",
      "quantity": 1,
      "unitPriceBeforeTax": 1000.0,
      "totalBeforeTax": 1000.0,
      "taxAmount": 0.0,
      "totalIncludingTax": 1000.0
    }
  ],
  "invoiceNote": "Optional invoice note"
}
```

### Required Fields

| Field | Type | Description | Valid Values |
|-------|------|-------------|-------------|
| `invoiceNumber` | string | Unique invoice number | Any unique string |
| `uniqueSerialNumber` | string | Unique serial number (UUID preferred) | UUID or unique string |
| `invoiceDate` | string | Invoice date (yyyy-MM-dd) | Date in yyyy-MM-dd format, not in future |
| `paymentType` | enum | Payment type | `LocalIncomeCash`, `LocalIncomeCredit`, `LocalGeneralSalesCash`, `LocalGeneralSalesCredit`, `LocalSpecialSalesCash`, `LocalSpecialSalesCredit` |
| `type` | enum | Invoice type | `Income`, `GeneralSales`, `SpecialSales` (default: `GeneralSales`) |
| `currency` | enum | Currency code | `JOD` (default), other currency codes |
| `supplier` | object | Supplier information | See structure below |
| `customer` | object | Customer information | See structure below |
| `invoiceTotals` | object | Invoice totals | See structure below |
| `invoiceDetails` | array | Invoice line items | See structure below, minimum 1 item |

### Supplier Structure (Required)

```json
{
  "supplier": {
    "taxVATNumber": "1234567890",              // Required
    "incomeSourceSequence": "62010",             // Required
    "registeredSupplierName": "Your Company Name"  // Required
  }
}
```

### Customer Structure

```json
{
  "customer": {
    "name": "Customer Name",                    // Required
    "identificationNumber": "987654321",        // Optional
    "identificationType": "NIN",                // Optional - Valid values: "NIN", "PN", "TN" (see notes below)
    "phoneNumber": "962791234567",            // Optional
    "city": "Amman",                           // Optional
    "postalCode": "11118"                      // Optional
  }
}
```

#### Important Notes on `identificationType`

**Valid Values:**

- `"NIN"` - National ID Number (الرقم الوطني)
- `"PN"` - Passport Number (رقم جواز السفر)
- `"TN"` - Tax Number (الرقم الضريبي)

**⚠️ Common Mistakes:**

- ❌ **DO NOT** use `"NationalID"` - use `"NIN"` instead
- ❌ **DO NOT** use `"PassportNumber"` - use `"PN"` instead
- ❌ **DO NOT** use `"TaxNumber"` - use `"TN"` instead
- ❌ **DO NOT** set the value to `null` - either omit the field entirely or use a valid value

**Usage Rules:**

- Both `identificationNumber` and `identificationType` are **optional fields**
- **IMPORTANT:** If you provide `identificationType`, you **MUST** also provide `identificationNumber`
- However, you can provide `identificationNumber` without `identificationType` (though not recommended)
- If you don't need customer identification, **omit both fields entirely** from the JSON
- When present, `identificationType` must contain one of the three valid values above
- You can set either field to `null` - the application will treat it as if the field is omitted

**Valid Examples:**

```json
// Example 1: With identification
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210",
    "identificationType": "NIN"
  }
}

// Example 2: Without identification (both fields omitted)
{
  "customer": {
    "name": "Ahmad Hassan"
  }
}
```

**Invalid Examples:**

```json
// ❌ WRONG: Using "NationalID" instead of "NIN"
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210",
    "identificationType": "NationalID"  // This will cause error!
  }
}

// ❌ WRONG: Setting identificationType to null
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationType": null  // This will cause error!
  }
}

// ❌ WRONG: Having identificationType without identificationNumber
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationType": "NIN"  // Error! identificationNumber is required when identificationType is provided
  }
}

// ✅ ACCEPTABLE: Having identificationNumber without identificationType
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210"  // OK, but not recommended
  }
}
```

### Invoice Totals Structure (Required)

```json
{
  "invoiceTotals": {
    "totalVATAmount": 160.0,              // Required
    "totalSpecialTaxAmount": 0.0,           // Required
    "totalBeforeDiscount": 1000.0,         // Required
    "totalInvoiceAmount": 1160.0,          // Required
    "totalDiscountAmount": 0.0,              // Required
    "finalPayableAmount": 1160.0            // Required
  }
}
```

### Invoice Details Structure (Required, minimum 1 item)

```json
{
  "invoiceDetails": [
    {
      "id": "LINE-1",                       // Required
      "taxCategory": "S",                    // Required: "Z" for Income, "S" for Sales
      "description": "Item Description",       // Required
      "quantity": 5,                        // Required: must be > 0
      "unitPriceBeforeTax": 200.0,          // Required
      "totalBeforeTax": 1000.0,             // Required
      "taxAmount": 160.0,                  // Required
      "totalIncludingTax": 1160.0,          // Required
      "discountAmount": 0.0,                 // Optional
      "specialTaxAmount": 0.0                // Optional
    }
  ]
}
```

### Complete Income Invoice Sample

```json
{
  "invoiceNumber": "INV-2024-001",
  "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
  "invoiceDate": "2024-01-15",
  "paymentType": "LocalIncomeCash",
  "type": "Income",
  "currency": "JOD",
  "supplier": {
    "taxVATNumber": "1234567890",
    "incomeSourceSequence": "62010",
    "registeredSupplierName": "Your Company Name"
  },
  "customer": {
    "name": "Ahmad Hassan",
    "phoneNumber": "962791234567"
  },
  "invoiceTotals": {
    "totalVATAmount": 0.0,
    "totalSpecialTaxAmount": 0.0,
    "totalBeforeDiscount": 1000.0,
    "totalInvoiceAmount": 1000.0,
    "totalDiscountAmount": 0.0,
    "finalPayableAmount": 1000.0
  },
  "invoiceDetails": [
    {
      "id": "LINE-1",
      "taxCategory": "Z",
      "description": "Consulting Services",
      "quantity": 10,
      "unitPriceBeforeTax": 100.0,
      "totalBeforeTax": 1000.0,
      "taxAmount": 0.0,
      "totalIncludingTax": 1000.0
    }
  ],
  "invoiceNote": "Professional consulting services"
}
```

### General Sales Invoice Sample

```json
{
  "invoiceNumber": "GEN-2024-001",
  "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440001",
  "invoiceDate": "2024-01-15",
  "paymentType": "LocalGeneralSalesCash",
  "type": "GeneralSales",
  "currency": "JOD",
  "supplier": {
    "taxVATNumber": "1234567890",
    "incomeSourceSequence": "62010",
    "registeredSupplierName": "Your Company Name"
  },
  "customer": {
    "name": "Retail Customer",
    "phoneNumber": "962791234567"
  },
  "invoiceTotals": {
    "totalVATAmount": 16.0,
    "totalSpecialTaxAmount": 0.0,
    "totalBeforeDiscount": 100.0,
    "totalInvoiceAmount": 116.0,
    "totalDiscountAmount": 0.0,
    "finalPayableAmount": 116.0
  },
  "invoiceDetails": [
    {
      "id": "LINE-1",
      "taxCategory": "S",
      "description": "Product A",
      "quantity": 5,
      "unitPriceBeforeTax": 20.0,
      "totalBeforeTax": 100.0,
      "taxAmount": 16.0,
      "totalIncludingTax": 116.0
    }
  ],
  "invoiceNote": "General sales invoice"
}
```

---

## Data Validation

The application implements strict validation for all JSON input to ensure data integrity and compliance with Jofotara requirements.

### Validation Rules

#### Date Validation
- **Format**: Must be in `yyyy-MM-dd` format (e.g., "2024-01-15")
- **Range**: Cannot be in the future (must be today or earlier)
- **Error Example**: `Property 'invoiceDate' must be in yyyy-MM-dd format. Got: 15/01/2024`

#### Numeric Validation
- **Quantities**: Must be positive integers (> 0)
- **Amounts**: Must be non-negative decimals (>= 0)
- **Error Example**: `Property 'quantity' must be greater than 0`

#### Required Fields Validation
- All required fields must be present and non-empty
- Nested objects (supplier, customer, invoiceTotals, invoiceDetails) must exist
- invoiceDetails array must contain at least one item
- **Error Example**: `Required property 'supplier' is missing`

#### Enum Validation
- Payment types must be valid `InvoicePaymentTypeCode` values
- Invoice types must be valid `InvoiceType` values
- Currency codes must be valid `CurrencyCode` values
- Tax categories must be valid `TaxCategoryCode` values
- Identification types must be valid `IdentificationType` values: `NIN`, `PN`, or `TN`
- **Error Example**: `Invalid InvoicePaymentTypeCode value 'Cash' for property 'paymentType'`
- **Error Example**: `Invalid IdentificationType value 'NationalID' for property 'identificationType'`

### Common Validation Errors and Solutions

#### Error: "Invalid payment type"
**Problem**: Using string values like "Cash" or "Credit" instead of enum values
**Solution**: Use proper enum values:
- Income invoices: `LocalIncomeCash` or `LocalIncomeCredit`
- General Sales: `LocalGeneralSalesCash` or `LocalGeneralSalesCredit`
- Special Sales: `LocalSpecialSalesCash` or `LocalSpecialSalesCredit`

#### Error: "Invoice date cannot be in the future"
**Problem**: Using a future date in the invoiceDate field
**Solution**: Use today's date or a past date in yyyy-MM-dd format

#### Error: "Required property 'taxCategory' is missing"
**Problem**: Missing taxCategory in invoice details
**Solution**: Add taxCategory to each invoice detail:
- Use "Z" for Income invoices (zero-rated)
- Use "S" for Sales invoices (standard rate)

#### Error: "invoiceDetails must contain at least one item"

**Problem**: Empty invoice details array
**Solution**: Ensure at least one item is present in the invoiceDetails array

#### Error: "Invalid IdentificationType value"

**Problem**: Using incorrect values for customer `identificationType` field

**Common Mistakes**:

- Using `"NationalID"` instead of `"NIN"`
- Using `"PassportNumber"` instead of `"PN"`
- Using `"TaxNumber"` instead of `"TN"`
- Setting the value to `null` instead of omitting the field

**Solution**: Use only these exact values:

- `"NIN"` for National ID Number (الرقم الوطني)
- `"PN"` for Passport Number (رقم جواز السفر)
- `"TN"` for Tax Number (الرقم الضريبي)

**Examples**:

```json
// ✅ CORRECT: Using "NIN"
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210",
    "identificationType": "NIN"
  }
}

// ✅ CORRECT: Omitting both identification fields
{
  "customer": {
    "name": "Ahmad Hassan"
  }
}

// ❌ WRONG: Using "NationalID"
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210",
    "identificationType": "NationalID"  // Error!
  }
}

// ❌ WRONG: Using null (though now accepted, it's not recommended)
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationType": null  // Now accepted but omitting the field is better
  }
}

// ❌ WRONG: Having identificationType without identificationNumber
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationType": "NIN"  // Error! Must provide identificationNumber
  }
}
```

#### Error: "Customer IdentificationNumber is required when IdentificationType is specified"

**Problem**: Providing `identificationType` without `identificationNumber`

**Solution**: If you specify an `identificationType`, you must also provide an `identificationNumber`. The rule is:

- Both fields are optional
- But if you provide `identificationType`, then `identificationNumber` becomes required

**Example**:

```json
// ❌ WRONG
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationType": "NIN"  // Missing identificationNumber!
  }
}

// ✅ CORRECT
{
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210",
    "identificationType": "NIN"
  }
}
```

### JSON Validation Tools

Before submitting invoices, you can validate your JSON using:

1. **Online JSON Validators**: Use tools like jsonlint.com to check syntax
2. **Application Validation**: Use the application's built-in validation by attempting to submit
3. **Sample Comparison**: Compare your JSON with the provided samples using `--sample`

---

## Output and Exit Codes

### Exit Codes

| Code | Name | Description | Required Action |
|------|------|-------------|-----------------|
| `0` | Success | Submission successful | None |
| `1` | ValidationError | Data validation failed | Review error messages and fix data |
| `2` | ApiError | API connection error | Check internet connection and API status |
| `3` | AuthenticationError | Invalid credentials | Verify ClientId and SecretKey |
| `4` | JsonParseError | JSON format error | Review JSON format and fix |
| `5` | ConfigurationError | Missing configuration | Review User Secrets setup |
| `6` | FileNotFoundError | File not found | Verify file path |
| `99` | UnexpectedError | Unexpected error | Review logs and contact support |

### Text Mode Output

#### Success

```
✓ Invoice submitted successfully
  Invoice Number: INV-20241122-143052
  QR Code: iVBORw0KGgoAAAANSUhEUgAA...
```

#### Failure

```
✗ ERROR: Invoice validation failed
  Exit Code: 1 (ValidationError)

Details:
  - InvoiceDate: Invoice date cannot be in the future. Got: 2025-12-31
  - Customer.TaxNumber: Required for SpecialSales invoices
```

### JSON Mode Output

#### Success

```json
{
  "success": true,
  "message": "Invoice submitted successfully",
  "invoiceNumber": "INV-20241122-143052",
  "qrCode": "iVBORw0KGgoAAAANSUhEUgAA...",
  "returnInvoiceNumber": null,
  "exitCode": 0
}
```

#### Failure

```json
{
  "success": false,
  "errorType": "ValidationError",
  "message": "Invoice validation failed",
  "errors": [
    {
      "field": null,
      "path": null,
      "message": "InvoiceDate cannot be in the future",
      "expectedValue": null,
      "actualValue": null
    }
  ],
  "exitCode": 1
}
```

---

## Integration with Other Systems

### Bash Script for Batch Processing

```bash
#!/bin/bash

# Configuration
INVOICE_DIR="/data/invoices/pending"
PROCESSED_DIR="/data/invoices/processed"
FAILED_DIR="/data/invoices/failed"
LOG_FILE="/var/log/efatorajo/process.log"

# Process all invoices
for invoice_file in "$INVOICE_DIR"/*.json; do
  if [ -f "$invoice_file" ]; then
    filename=$(basename "$invoice_file")

    echo "[$(date)] Processing: $filename" >> "$LOG_FILE"

    # Submit invoice using dotnet command
    dotnet /opt/efatorajo/EFatoraJoConsoleApp.dll \
      --invoice-file "$invoice_file" \
      --output-format json > "/tmp/result_$filename"

    exit_code=$?

    if [ $exit_code -eq 0 ]; then
      # Success
      invoice_number=$(cat "/tmp/result_$filename" | jq -r '.invoiceNumber')
      echo "[$(date)] SUCCESS: $filename -> $invoice_number" >> "$LOG_FILE"
      mv "$invoice_file" "$PROCESSED_DIR/"
    else
      # Failure
      error_type=$(cat "/tmp/result_$filename" | jq -r '.errorType')
      echo "[$(date)] FAILED: $filename (Exit Code: $exit_code, Type: $error_type)" >> "$LOG_FILE"
      mv "$invoice_file" "$FAILED_DIR/"
      mv "/tmp/result_$filename" "$FAILED_DIR/error_$filename"
    fi

    rm -f "/tmp/result_$filename"
  fi
done
```

### PowerShell Script for Batch Processing

```powershell
# Configuration
$InvoiceDir = "C:\Data\Invoices\Pending"
$ProcessedDir = "C:\Data\Invoices\Processed"
$FailedDir = "C:\Data\Invoices\Failed"
$LogFile = "C:\Logs\EFatoraJo\process.log"

# Process all invoices
Get-ChildItem -Path $InvoiceDir -Filter "*.json" | ForEach-Object {
    $invoiceFile = $_.FullName
    $filename = $_.Name
    $resultFile = Join-Path $env:TEMP "result_$filename"

    Add-Content -Path $LogFile -Value "[$(Get-Date)] Processing: $filename"

    # Submit invoice using dotnet command
    dotnet C:\EFatoraJo\EFatoraJoConsoleApp.dll --invoice-file $invoiceFile --output-format json | Out-File $resultFile

    if ($LASTEXITCODE -eq 0) {
        # Success
        $result = Get-Content $resultFile | ConvertFrom-Json
        $invoiceNumber = $result.invoiceNumber
        Add-Content -Path $LogFile -Value "[$(Get-Date)] SUCCESS: $filename -> $invoiceNumber"
        Move-Item $invoiceFile -Destination $ProcessedDir
    }
    else {
        # Failure
        $result = Get-Content $resultFile | ConvertFrom-Json
        $errorType = $result.errorType
        Add-Content -Path $LogFile -Value "[$(Get-Date)] FAILED: $filename (Exit Code: $LASTEXITCODE, Type: $errorType)"
        Move-Item $invoiceFile -Destination $FailedDir
        Move-Item $resultFile -Destination (Join-Path $FailedDir "error_$filename")
    }

    Remove-Item $resultFile -ErrorAction SilentlyContinue
}
```

### Python Integration Example

```python
import subprocess
import json
import logging

def submit_invoice(invoice_data):
    """
    Submit invoice to Jofotara

    Args:
        invoice_data: dict - Invoice data

    Returns:
        dict - Result from application
    """
    try:
        # Convert data to JSON
        invoice_json = json.dumps(invoice_data)

        # Execute application using dotnet command
        result = subprocess.run(
            ['dotnet', 'EFatoraJoConsoleApp.dll', '--invoice-json', invoice_json, '--output-format', 'json'],
            capture_output=True,
            text=True,
            timeout=30,
            cwd='EFatoraJoConsoleApp/bin/Release/net8.0'  # Adjust path as needed
        )

        # Parse result
        output = json.loads(result.stdout)

        if result.returncode == 0:
            logging.info(f"Invoice submitted successfully: {output['invoiceNumber']}")
            return {
                'success': True,
                'invoice_number': output['invoiceNumber'],
                'qr_code': output['qrCode']
            }
        else:
            logging.error(f"Invoice submission failed: {output['errorType']}")
            return {
                'success': False,
                'error_type': output['errorType'],
                'errors': output['errors'],
                'exit_code': result.returncode
            }

    except subprocess.TimeoutExpired:
        logging.error("Invoice submission timed out")
        return {'success': False, 'error': 'Timeout'}
    except Exception as e:
        logging.error(f"Unexpected error: {str(e)}")
        return {'success': False, 'error': str(e)}

# Usage example - Updated with complete JSON structure
invoice = {
    "invoiceNumber": "INV-2024-001",
    "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
    "invoiceDate": "2024-01-15",
    "paymentType": "LocalIncomeCash",
    "type": "Income",
    "currency": "JOD",
    "supplier": {
        "taxVATNumber": "1234567890",
        "incomeSourceSequence": "62010",
        "registeredSupplierName": "Your Company Name"
    },
    "customer": {
        "name": "Ahmad Hassan",
        "phoneNumber": "962791234567"
    },
    "invoiceTotals": {
        "totalVATAmount": 0.0,
        "totalSpecialTaxAmount": 0.0,
        "totalBeforeDiscount": 1000.0,
        "totalInvoiceAmount": 1000.0,
        "totalDiscountAmount": 0.0,
        "finalPayableAmount": 1000.0
    },
    "invoiceDetails": [
        {
            "id": "LINE-1",
            "taxCategory": "Z",
            "description": "Consulting Services",
            "quantity": 10,
            "unitPriceBeforeTax": 100.0,
            "totalBeforeTax": 1000.0,
            "taxAmount": 0.0,
            "totalIncludingTax": 1000.0
        }
    ],
    "invoiceNote": "Professional consulting services"
}

result = submit_invoice(invoice)
print(result)
# Usage example
invoice = {
    "invoiceNumber": "INV-2024-001",
    "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
    "invoiceDate": "2024-01-15",
    "invoiceType": "Income",
    "paymentType": "Cash",
    "customer": {
        "name": "Ahmad Hassan",
        "mobileNumber": "962791234567"
    },
    "invoiceLines": [
        {
            "itemName": "Consulting Services",
            "itemQuantity": 10,
            "itemPrice": 100.00,
            "totalAmount": 1000.00
        }
    ],
    "totalInvoiceAmount": 1000.00
}

result = submit_invoice(invoice)
print(result)
```

### Node.js Integration Example

```javascript
const { exec } = require('child_process');
// Usage example - Updated with complete JSON structure
const invoice = {
  invoiceNumber: "INV-2024-001",
  uniqueSerialNumber: "550e8400-e29b-41d4-a716-446655440000",
  invoiceDate: "2024-01-15",
  paymentType: "LocalIncomeCash",
  type: "Income",
  currency: "JOD",
  supplier: {
    taxVATNumber: "1234567890",
    incomeSourceSequence: "62010",
    registeredSupplierName: "Your Company Name"
  },
  customer: {
    name: "Ahmad Hassan",
    phoneNumber: "962791234567"
  },
  invoiceTotals: {
    totalVATAmount: 0.0,
    totalSpecialTaxAmount: 0.0,
    totalBeforeDiscount: 1000.0,
    totalInvoiceAmount: 1000.0,
    totalDiscountAmount: 0.0,
    finalPayableAmount: 1000.0
  },
  invoiceDetails: [
    {
      id: "LINE-1",
      taxCategory: "Z",
      description: "Consulting Services",
      quantity: 10,
      unitPriceBeforeTax: 100.0,
      totalBeforeTax: 1000.0,
      taxAmount: 0.0,
      totalIncludingTax: 1000.0
    }
  ],
  invoiceNote: "Professional consulting services"
};

submitInvoice(invoice).then(result => console.log(result));
const util = require('util');
const execPromise = util.promisify(exec);

async function submitInvoice(invoiceData) {
  try {
    const invoiceJson = JSON.stringify(invoiceData);

    // Execute using dotnet command
    const { stdout, stderr } = await execPromise(
      `dotnet EFatoraJoConsoleApp.dll --invoice-json '${invoiceJson}' --output-format json`,
      {
        timeout: 30000,
        cwd: 'EFatoraJoConsoleApp/bin/Release/net8.0'  // Adjust path as needed
      }
    );

    const result = JSON.parse(stdout);

    if (result.success) {
      console.log(`Invoice submitted: ${result.invoiceNumber}`);
      return {
        success: true,
        invoiceNumber: result.invoiceNumber,
        qrCode: result.qrCode
      };
    } else {
      console.error(`Failed: ${result.errorType}`);
      return {
        success: false,
        errorType: result.errorType,
        errors: result.errors
      };
    }
  } catch (error) {
    console.error(`Error: ${error.message}`);
    return {
      success: false,
      error: error.message
    };
  }
}

// Usage example
const invoice = {
  invoiceNumber: "INV-2024-001",
  uniqueSerialNumber: "550e8400-e29b-41d4-a716-446655440000",
  invoiceDate: "2024-01-15",
  invoiceType: "Income",
  paymentType: "Cash",
  customer: {
    name: "Ahmad Hassan",
    mobileNumber: "962791234567"
  },
  invoiceLines: [
    {
      itemName: "Consulting Services",
      itemQuantity: 10,
      itemPrice: 100.00,
      totalAmount: 1000.00
    }
  ],
  totalInvoiceAmount: 1000.00
};

submitInvoice(invoice).then(result => console.log(result));
```

---

## Systemd Service Setup (Linux)

### 1. Create Service User (Optional but Recommended)

```bash
# Create dedicated user for running the service
sudo useradd -r -s /bin/false efatorajo

# Create necessary directories
sudo mkdir -p /opt/efatorajo
sudo mkdir -p /var/log/efatorajo

# Set permissions
sudo chown -R efatorajo:efatorajo /opt/efatorajo
sudo chown -R efatorajo:efatorajo /var/log/efatorajo
```

### 2. Create Processing Script

```bash
sudo nano /opt/efatorajo/process-invoices.sh
```

**Script Content:**

```bash
#!/bin/bash
# Invoice processing script

INVOICE_DIR="/opt/efatorajo/data/pending"
PROCESSED_DIR="/opt/efatorajo/data/processed"
FAILED_DIR="/opt/efatorajo/data/failed"
LOG_FILE="/var/log/efatorajo/process.log"

# Create directories if they don't exist
mkdir -p "$INVOICE_DIR" "$PROCESSED_DIR" "$FAILED_DIR"

# Process all invoices
for invoice_file in "$INVOICE_DIR"/*.json; do
  if [ -f "$invoice_file" ]; then
    filename=$(basename "$invoice_file")
    echo "[$(date)] Processing: $filename" >> "$LOG_FILE"

    # Submit invoice using dotnet
    dotnet /opt/efatorajo/EFatoraJoConsoleApp.dll \
      --invoice-file "$invoice_file" \
      --output-format json > "/tmp/result_$filename"

    if [ $? -eq 0 ]; then
      mv "$invoice_file" "$PROCESSED_DIR/"
      echo "[$(date)] SUCCESS: $filename" >> "$LOG_FILE"
    else
      mv "$invoice_file" "$FAILED_DIR/"
      echo "[$(date)] FAILED: $filename" >> "$LOG_FILE"
    fi

    rm -f "/tmp/result_$filename"
  fi
done

# Sleep for 60 seconds before next batch
sleep 60
```

**Make script executable:**

```bash
sudo chmod +x /opt/efatorajo/process-invoices.sh
```

### 3. Create Systemd Service File

```bash
sudo nano /etc/systemd/system/efatorajo-processor.service
```

**File Content:**

```ini
[Unit]
Description=EFatoraJo Invoice Processor
After=network.target

[Service]
Type=simple
User=efatorajo
WorkingDirectory=/opt/efatorajo
ExecStart=/opt/efatorajo/process-invoices.sh
Restart=always
RestartSec=10

# Environment variables (set your credentials here)
Environment="EFATORA_CLIENT_ID=your-client-id"
Environment="EFATORA_SECRET_KEY=your-secret-key"
Environment="EFATORA_TAX_NUMBER=1234567890"
Environment="EFATORA_ACTIVITY_CODE=62010"
Environment="EFATORA_SUPPLIER_NAME=Your Company Name"

# Logging
StandardOutput=append:/var/log/efatorajo/service.log
StandardError=append:/var/log/efatorajo/error.log

[Install]
WantedBy=multi-user.target
```

### Enable Service

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service
sudo systemctl enable efatorajo-processor

# Start service
sudo systemctl start efatorajo-processor

# Check status
sudo systemctl status efatorajo-processor

# View logs
sudo journalctl -u efatorajo-processor -f
```

---

## macOS LaunchAgent Setup

### 1. Create Processing Script

```bash
nano ~/Library/Application\ Support/EFatoraJo/process-invoices.sh
```

**Script Content:**

```bash
#!/bin/bash
# Invoice processing script for macOS

INVOICE_DIR="$HOME/Library/Application Support/EFatoraJo/data/pending"
PROCESSED_DIR="$HOME/Library/Application Support/EFatoraJo/data/processed"
FAILED_DIR="$HOME/Library/Application Support/EFatoraJo/data/failed"
LOG_FILE="$HOME/Library/Logs/EFatoraJo/process.log"

# Create directories if they don't exist
mkdir -p "$INVOICE_DIR" "$PROCESSED_DIR" "$FAILED_DIR"
mkdir -p "$HOME/Library/Logs/EFatoraJo"

# Process all invoices
for invoice_file in "$INVOICE_DIR"/*.json; do
  if [ -f "$invoice_file" ]; then
    filename=$(basename "$invoice_file")
    echo "[$(date)] Processing: $filename" >> "$LOG_FILE"

    # Submit invoice using dotnet
    dotnet "$HOME/Applications/EFatoraJo/EFatoraJoConsoleApp.dll" \
      --invoice-file "$invoice_file" \
      --output-format json > "/tmp/result_$filename"

    if [ $? -eq 0 ]; then
      mv "$invoice_file" "$PROCESSED_DIR/"
      echo "[$(date)] SUCCESS: $filename" >> "$LOG_FILE"
    else
      mv "$invoice_file" "$FAILED_DIR/"
      echo "[$(date)] FAILED: $filename" >> "$LOG_FILE"
    fi

    rm -f "/tmp/result_$filename"
  fi
done
```

**Make executable:**

```bash
chmod +x ~/Library/Application\ Support/EFatoraJo/process-invoices.sh
```

### 2. Create LaunchAgent Plist

```bash
nano ~/Library/LaunchAgents/com.efatorajo.processor.plist
```

**File Content:**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.efatorajo.processor</string>

    <key>ProgramArguments</key>
    <array>
        <string>/bin/bash</string>
        <string>-c</string>
        <string>~/Library/Application Support/EFatoraJo/process-invoices.sh</string>
    </array>

    <key>StartInterval</key>
    <integer>60</integer>

    <key>RunAtLoad</key>
    <true/>

    <key>KeepAlive</key>
    <false/>

    <key>StandardOutPath</key>
    <string>~/Library/Logs/EFatoraJo/service.log</string>

    <key>StandardErrorPath</key>
    <string>~/Library/Logs/EFatoraJo/error.log</string>

    <key>EnvironmentVariables</key>
    <dict>
        <key>EFATORA_CLIENT_ID</key>
        <string>your-client-id</string>
        <key>EFATORA_SECRET_KEY</key>
        <string>your-secret-key</string>
        <key>EFATORA_TAX_NUMBER</key>
        <string>1234567890</string>
        <key>EFATORA_ACTIVITY_CODE</key>
        <string>62010</string>
        <key>EFATORA_SUPPLIER_NAME</key>
        <string>Your Company Name</string>
    </dict>
</dict>
</plist>
```

### 3. Load and Start LaunchAgent

```bash
# Load the agent
launchctl load ~/Library/LaunchAgents/com.efatorajo.processor.plist

# Start immediately
launchctl start com.efatorajo.processor

# Check status
launchctl list | grep efatorajo

# View logs
tail -f ~/Library/Logs/EFatoraJo/process.log
```

### 4. Unload LaunchAgent (if needed)

```bash
launchctl unload ~/Library/LaunchAgents/com.efatorajo.processor.plist
```

---

## Windows Service Setup

### 1. Create Processing Script

Create `C:\EFatoraJo\process-invoices.bat`:

```batch
@echo off
REM Invoice processing script for Windows

set INVOICE_DIR=C:\EFatoraJo\data\pending
set PROCESSED_DIR=C:\EFatoraJo\data\processed
set FAILED_DIR=C:\EFatoraJo\data\failed
set LOG_FILE=C:\Logs\EFatoraJo\process.log

REM Create directories if they don't exist
if not exist "%INVOICE_DIR%" mkdir "%INVOICE_DIR%"
if not exist "%PROCESSED_DIR%" mkdir "%PROCESSED_DIR%"
if not exist "%FAILED_DIR%" mkdir "%FAILED_DIR%"
if not exist "C:\Logs\EFatoraJo" mkdir "C:\Logs\EFatoraJo"

REM Process all invoices
for %%f in ("%INVOICE_DIR%\*.json") do (
    echo [%date% %time%] Processing: %%~nxf >> "%LOG_FILE%"

    C:\EFatoraJo\EFatoraJoConsoleApp.exe --invoice-file "%%f" --output-format json > "%TEMP%\result_%%~nxf"

    if %ERRORLEVEL% EQU 0 (
        move "%%f" "%PROCESSED_DIR%\" >nul
        echo [%date% %time%] SUCCESS: %%~nxf >> "%LOG_FILE%"
    ) else (
        move "%%f" "%FAILED_DIR%\" >nul
        echo [%date% %time%] FAILED: %%~nxf >> "%LOG_FILE%"
    )

    del "%TEMP%\result_%%~nxf" 2>nul
)

REM Sleep for 60 seconds
timeout /t 60 /nobreak >nul
```

### 2. Using NSSM (Non-Sucking Service Manager)

**Download and Install NSSM:**

1. Download from: https://nssm.cc/download
2. Extract to `C:\Tools\nssm\`

**Install Service:**

```powershell
# Navigate to NSSM directory
cd C:\Tools\nssm\win64

# Install service
.\nssm.exe install EFatoraJoProcessor "C:\EFatoraJo\process-invoices.bat"

# Configure working directory
.\nssm.exe set EFatoraJoProcessor AppDirectory "C:\EFatoraJo"

# Configure logging
.\nssm.exe set EFatoraJoProcessor AppStdout "C:\Logs\EFatoraJo\service.log"
.\nssm.exe set EFatoraJoProcessor AppStderr "C:\Logs\EFatoraJo\error.log"

# Configure auto-restart
.\nssm.exe set EFatoraJoProcessor AppRestartDelay 10000

# Note: Environment variables are not currently supported by the application
# Use .NET User Secrets instead (see Configuration section)
# .\nssm.exe set EFatoraJoProcessor AppEnvironmentExtra EFATORA_CLIENT_ID=your-client-id EFATORA_SECRET_KEY=your-secret-key

# Start service
.\nssm.exe start EFatoraJoProcessor

# Check status
.\nssm.exe status EFatoraJoProcessor
```

### 3. Managing the Service

```powershell
# Stop service
nssm stop EFatoraJoProcessor

# Restart service
nssm restart EFatoraJoProcessor

# Remove service
nssm remove EFatoraJoProcessor confirm
```

---

## Monitoring and Logging

### Configure Logging

```bash
# Create log directories
sudo mkdir -p /var/log/efatorajo
sudo chown efatorajo:efatorajo /var/log/efatorajo

# Setup log rotation
sudo nano /etc/logrotate.d/efatorajo
```

### Logrotate Configuration

```
/var/log/efatorajo/*.log {
    daily
    rotate 30
    compress
    delaycompress
    notifempty
    create 0640 efatorajo efatorajo
    sharedscripts
    postrotate
        systemctl reload efatorajo-processor > /dev/null 2>&1 || true
    endscript
}
```

### Performance Monitoring

```bash
# View live logs
tail -f /var/log/efatorajo/process.log

# Search for errors
grep "FAILED" /var/log/efatorajo/process.log

# Success/Failure statistics
echo "Successful: $(grep -c 'SUCCESS' /var/log/efatorajo/process.log)"
echo "Failed: $(grep -c 'FAILED' /var/log/efatorajo/process.log)"
```

---

## Security and Best Practices

### 1. Credential Protection

- ✅ **Use User Secrets** for storing credentials
- ✅ **Never commit** ClientId and SecretKey to code or Git
- ✅ **Use environment variables** in specific production environments
- ✅ **Rotate credentials** regularly

### 2. Data Validation

- ✅ **Validate JSON format** before submission
- ✅ **Validate dates** (must not be in the future)
- ✅ **Validate numbers** (must be positive)
- ✅ **Validate required fields** based on invoice type

### 3. Error Handling

- ✅ **Log all errors** with full details
- ✅ **Retry** on network errors
- ✅ **Don't retry** on validation errors
- ✅ **Send alerts** on repeated failures

### 4. Performance

- ✅ **Use batch processing** for multiple invoices
- ✅ **Set appropriate timeout** values
- ✅ **Monitor resource usage** (CPU, Memory)
- ✅ **Archive old logs** regularly

### 5. Backup

- ✅ **Keep copies** of all submitted invoices
- ✅ **Keep copies** of results and QR Codes
- ✅ **Backup configuration** regularly
- ✅ **Test data recovery** periodically

---

## Troubleshooting

### Issue: AuthenticationError (Exit Code 3)

**Symptoms:**
```
✗ ERROR: Authentication failed - please check your ClientId and SecretKey
Exit Code: 3 (AuthenticationError)
```

**Solutions:**
1. Verify ClientId and SecretKey are correct
2. Ensure account is active on Jofotara
3. Check User Secrets:
   ```bash
   dotnet user-secrets list
   ```

---

### Issue: JsonParseError (Exit Code 4)

**Symptoms:**
```
✗ ERROR: Failed to parse invoice JSON
Exit Code: 4 (JsonParseError)

Details:
  - Property 'invoiceDate' must be in yyyy-MM-dd format
```

**Solutions:**
1. Validate JSON using a JSON validator
2. Ensure date format: `yyyy-MM-dd`
3. Ensure correct data types (numbers as numbers, strings as strings)
4. Use `--sample` to display correct format

---

### Issue: ValidationError (Exit Code 1)

**Symptoms:**
```
✗ ERROR: Invoice validation failed
Exit Code: 1 (ValidationError)

Details:
  - Customer.TaxNumber: Required for SpecialSales invoices
```

**Solutions:**
1. Review error messages carefully
2. Ensure all required fields are present based on invoice type:
   - **SpecialSales**: Requires TaxNumber
   - **Income**: Does not require TaxNumber
   - **GeneralSales**: Does not require TaxNumber
3. Verify values (not negative, dates correct, etc.)


---

### Issue: Invalid Payment Type (ValidationError)

**Symptoms:**
```
✗ ERROR: Invoice validation failed
Exit Code: 1 (ValidationError)

Details:
  - Invalid InvoicePaymentTypeCode value 'Cash' for property 'paymentType'
```

**Solutions:**
1. Use proper enum values for paymentType:
   - Income invoices: `LocalIncomeCash` or `LocalIncomeCredit`
   - General Sales: `LocalGeneralSalesCash` or `LocalGeneralSalesCredit`
   - Special Sales: `LocalSpecialSalesCash` or `LocalSpecialSalesCredit`
2. Update your JSON templates and scripts to use correct values

---

### Issue: Invoice Already Submitted (ApiError)

**Symptoms:**
```
Invoice was already submitted. Proceeding with return invoice submission if provided.
```

**Behavior:**

- When submitting an invoice that was previously submitted, the application will detect this
- If you're also submitting a return invoice (using `--return-file` or `--return-json`), the application will automatically skip re-submitting the original invoice and proceed directly to submitting the return invoice
- This allows you to submit return invoices for already-submitted invoices without errors

**Example:**

```bash
# This will work even if invoice.json was already submitted
dotnet EFatoraJoConsoleApp.dll \
  --invoice-file invoice.json \
  --return-file return.json \
  --output-format json
```

**Result:**

- Original invoice: Skipped (already submitted)
- Return invoice: Submitted successfully
- Exit code: 0 (Success)

---

### Issue: Missing Required Fields (ValidationError)

**Symptoms:**
```
✗ ERROR: Invoice validation failed
Exit Code: 1 (ValidationError)

Details:
  - Required property 'supplier' is missing
  - Required property 'invoiceTotals' is missing
  - Required property 'invoiceDetails' is missing
```

**Solutions:**
1. Ensure all required top-level objects are present in your JSON:
   - `supplier`: with taxVATNumber, incomeSourceSequence, registeredSupplierName
   - `customer`: with at least name
   - `invoiceTotals`: with all required total fields
   - `invoiceDetails`: array with at least one item
2. Use the complete JSON structure shown in the "Required JSON Format" section
3. Compare with samples using `--sample` command

---

### Issue: ConfigurationError (Exit Code 5)

**Symptoms:**
```
✗ ERROR: Missing required configuration in user secrets
Exit Code: 5 (ConfigurationError)

Details:
  - Missing or empty: ClientId
  - Missing or empty: SecretKey
  - Missing or empty: Supplier:TaxVATNumber
```

**Solutions:**
1. Set up User Secrets correctly:
   ```bash
   dotnet user-secrets set "ClientId" "YOUR_CLIENT_ID_HERE"
   dotnet user-secrets set "SecretKey" "YOUR_SECRET_KEY_HERE"
   dotnet user-secrets set "Supplier:TaxVATNumber" "1234567890"
   dotnet user-secrets set "Supplier:IncomeSourceSequence" "62010"
   dotnet user-secrets set "Supplier:RegisteredSupplierName" "Your Company Name"
   ```
2. Verify configuration:
   ```bash
   dotnet user-secrets list
   ```
3. Ensure you're in the correct project directory when setting secrets
---

## Frequently Asked Questions (FAQ)

### Q: Can I use the application without User Secrets?

**A:** Currently, the application only supports .NET User Secrets for configuration. Environment variables are not automatically loaded by the application. You must use User Secrets to store your credentials and supplier information.

---

### Q: What are the maximum limits for invoice size?

**A:** The application does not impose limits on invoice size, but Jofotara API limits should be considered (typically reasonable for normal invoices).

---

### Q: Does the application support batch submission?

**A:** Not directly. Use a script for batch processing (see "Integration with Other Systems" section).

---

### Q: How do I handle failed invoices?

**A:**
1. Review error messages in output
2. Fix the issue in the data
3. Resubmit
4. Keep failed invoices in a separate folder for review

---

### Q: Can I use the application in a Docker environment?

**A:** Yes, you can create a Docker image for the application. Example:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "EFatoraJoConsoleApp.dll"]
```

---

## Support and Contact

### Getting Help

- **Documentation:** Review this guide first
- **Examples:** Use `--sample` to display JSON samples
- **Help:** Use `--help` to display all options

### Reporting Issues

When reporting an issue, please include:
1. Application version (`--version`)
2. Operating system and version
3. Complete error message
4. Sample JSON used (without sensitive data)
5. Exit Code
6. Relevant logs

---

## Version Updates

### Version 1.0.0 (Current)

**Features:**
- Submit invoices from JSON (string, file, stdin)
- Support for return invoices
- JSON and Text output
- Unified exit codes (0-6, 99)
- Strict data validation
- Embedded JSON samples

---

## Conclusion

This application is ready for production use with:

✅ **High Security** - User Secrets for credential protection
✅ **Strict Validation** - Two-tier validation
✅ **Automatable Output** - JSON output and Exit codes
✅ **Easy Integration** - Works with any programming language
✅ **High Reliability** - Comprehensive error handling
✅ **Scalability** - Supports batch processing

---

**Release Date:** November 2024
**Last Updated:** November 2024 (Updated with comprehensive JSON structure and validation details)
**Last Updated:** November 2024
**Version:** 1.0.0
