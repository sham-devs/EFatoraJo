# EFatoraJo Console Application

## 1. Overview

A command-line application for submitting electronic invoices to the Jordanian e-invoicing system (Jofotara). The application supports sending invoices and return invoices from JSON files with strict validation and machine-readable output.

**Version:** 1.0.1  
**Platform:** .NET 8.0  
**License:** Per project agreement

## 2. Features

- Submit sales invoices and return invoices to Jofotara
- Support for Income, General Sales, and Special Sales invoice types
- Strict data validation with detailed error messages
- JSON and text output formats
- Interactive mode for testing and development
- Comprehensive exit codes for automation
- Built-in JSON samples
- Cross-platform support (Windows, Linux, macOS)

## 3. System Requirements

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

## 4. Installation

### Build the Application

```bash
# Build in Release mode
dotnet build -c Release

# The output will be in:
# EFatoraJoConsoleApp\bin\Release\net8.0\
```

### Running the Application

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

### Installing .NET 8.0 Runtime (if not installed)

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

## 5. Configuration

**Important:** Never store credentials in files or Git. Use environment variables or secure credential management.

### Using .NET User Secrets (Recommended for Development)

**Prerequisites:** Requires .NET SDK installed

```bash
# Navigate to application directory
cd /path/to/EFatoraJo/

# Configure credentials (used by interactive mode)
dotnet user-secrets set "EFatora:ClientId" "YOUR_CLIENT_ID_HERE"
dotnet user-secrets set "EFatora:SecretKey" "YOUR_SECRET_KEY_HERE"

# Configure supplier information (used by interactive mode)
dotnet user-secrets set "EFatora:Supplier:TaxNumber" "1234567890"
dotnet user-secrets set "EFatora:Supplier:ActivityCode" "62010"
dotnet user-secrets set "EFatora:Supplier:Name" "Your Company Name"
```

### Using Environment Variables (Production Only)

For production environments, you can pass credentials directly via command line parameters:

```bash
# Using environment variables
export EFATORA_CLIENT_ID="YOUR_CLIENT_ID_HERE"
export EFATORA_SECRET_KEY="YOUR_SECRET_KEY_HERE"

# Then use in command
EFatoraJoConsoleApp --invoice-file invoice.json --client-id "$EFATORA_CLIENT_ID" --secret-key "$EFATORA_SECRET_KEY"
```

### Verify Setup

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

## 6. CLI Usage

### Quick Start

#### Submit Invoice
```bash
EFatoraJoConsoleApp --invoice-file invoice.json --client-id "YOUR_CLIENT_ID" --secret-key "YOUR_SECRET_KEY"
```

#### Submit Return Invoice
```bash
EFatoraJoConsoleApp --return-file return.json --client-id "YOUR_CLIENT_ID" --secret-key "YOUR_SECRET_KEY"
```

### Available Commands

#### Help and Information

**Display Help:**
```bash
EFatoraJoConsoleApp --help
EFatoraJoConsoleApp -h
EFatoraJoConsoleApp -?
```

**Display Version:**
```bash
EFatoraJoConsoleApp --version
EFatoraJoConsoleApp -v
```

#### Display Sample JSON

**Income Invoice Sample:**
```bash
EFatoraJoConsoleApp --sample income
```

**General Sales Invoice Sample:**
```bash
EFatoraJoConsoleApp --sample general
```

**Special Sales Invoice Sample:**
```bash
EFatoraJoConsoleApp --sample special
```

**Return Invoice Sample:**
```bash
EFatoraJoConsoleApp --sample return
```

#### Submit Invoices

**Required Parameters:**

| Parameter | Description | Required? |
|-----------|-------------|-----------|
| `--invoice-file <path>` | Path to sales invoice JSON file | Yes (or use `--return-file`) |
| `--return-file <path>` | Path to return invoice JSON file | Yes (or use `--invoice-file`) |
| `--client-id <id>` | Client ID from Jofotara | Yes |
| `--secret-key <key>` | Secret Key from Jofotara | Yes |
| `--output-format <json|text>` | Output format (default: `json`) | No |
| `--verbose` | Show introductory header in text mode | No |

**Important Rules:**
- Must use `--invoice-file` **OR** `--return-file` (not both)
- Must provide `--client-id` and `--secret-key` with every request
- Credentials are NOT stored in the application for security reasons
- Default output is JSON; use `--output-format text` for plain text
- `--verbose` only affects text mode (adds a short header)

## 7. JSON Structure

### Required JSON Structure

The application requires a complete JSON structure that includes all necessary objects and fields.

#### Sales Invoice Structure

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

#### Return Invoice Structure

```json
{
  "originalInvoiceNumber": "INV-2025-001234",
  "returnInvoiceNumber": "RET-INV-2025-001234",
  "returnReason": "Customer request / item returned",
  "returnUUID": "21a105e5-4f52-4bb3-8b8b-38cdbbb6da3c",
  "uniqueSerialNumber": "c3f2a1b9-7a11-4f5f-a1c2-8e0d9f7c1234",
  "invoiceDate": "2025-01-15",
  "paymentType": "SameAsOriginal",
  "originalPaymentType": "LocalGeneralSalesCash",
  "originalInvoiceType": "GeneralSales",
  "type": "SalesReturn",
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
      "taxCategory": "Z",
      "description": "Item Description",
      "quantity": 2,
      "unitPriceBeforeTax": 50.0,
      "totalBeforeTax": 100.0,
      "taxAmount": 16.0,
      "totalIncludingTax": 116.0
    }
  ]
}
```

### Required Fields (Sales)

| Field | Type | Description | Valid Values |
|-------|------|-------------|-------------|
| `invoiceNumber` | string | Unique invoice number | Any unique string |
| `uniqueSerialNumber` | string | Unique serial number (UUID preferred) | UUID or unique string |
| `invoiceDate` | string | Invoice date (yyyy-MM-dd) | Not in the future |
| `paymentType` | enum | Payment type | `LocalIncomeCash`, `LocalIncomeCredit`, `LocalGeneralSalesCash`, `LocalGeneralSalesCredit`, `LocalSpecialSalesCash`, `LocalSpecialSalesCredit` |
| `type` | enum | Invoice type | `Income`, `GeneralSales`, `SpecialSales` (default: `GeneralSales`) |
| `currency` | enum | Currency code | `JOD` (default) |
| `supplier` | object | Supplier information | See structure below |
| `customer` | object | Customer information | See structure below |
| `invoiceTotals` | object | Invoice totals | See structure below |
| `invoiceDetails` | array | Invoice line items | Minimum 1 item |

### Required Fields (Return)

| Field | Type | Description |
|-------|------|-------------|
| `originalInvoiceNumber` | string | Required; link to the original invoice |
| `returnInvoiceNumber` | string | Required; must be supplied (no auto-generation) |
| `returnReason` | string | Required |
| `uniqueSerialNumber` | string | Required; UUID of the original invoice |
| `returnUUID` | string | Optional; UUID for the return invoice (auto-generated if omitted) |
| `invoiceDate` | string | Return invoice date (yyyy-MM-dd) |
| `type` | literal | Must be `"SalesReturn"` |
| `paymentType` | string | Either a valid payment enum or the sentinel `"SameAsOriginal"` (requires `originalPaymentType`) |
| `originalPaymentType` | enum | Required when `paymentType` is `"SameAsOriginal"` |
| `originalInvoiceType` | enum | Required; type of the original invoice |
| `invoiceTotals` | object | Totals from the original invoice (positive); converter applies negatives |
| `invoiceDetails` | array | Line items from the original invoice (positive); converter applies negatives |

### Customer Identification

For customer identification, use these exact values:

- `"NIN"` for National ID Number
- `"PN"` for Passport Number
- `"TN"` for Tax Number

**Important:** Both `identificationNumber` and `identificationType` are optional, but if you provide `identificationType`, you must also provide `identificationNumber`.

## 8. Interactive Mode

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
3. **Guided Submission**: Shows a summary, then asks to submit or skip
4. **Return Invoice Support**: Optionally creates and submits return invoices after confirmation
5. **Real-time Feedback**: Uses the same formatter as CLI for success/error output

### Interactive Mode Workflow

1. Run the application without arguments
2. Select invoice type (1-3):
   - 1: Income Invoice ( )
   - 2: General Sales Invoice (  )
   - 3: Special Sales Invoice (  )
3. Application generates a random invoice with test data
4. Review the generated invoice details
5. Choose to submit or skip
6. Optionally create a return invoice (y/n), then submit or skip
7. Repeat or exit (0)

### Interactive Mode Configuration

The interactive mode uses the same configuration as command-line mode (User Secrets). Make sure your credentials are properly configured before using interactive mode.

## 9. Usage Examples

### Example 1: Submit Invoice from File

**Windows:**

```powershell
EFatoraJoConsoleApp.exe --invoice-file "C:\invoices\inv001.json" --client-id "ABC123" --secret-key "xyz789secret"
```

**Linux/macOS:**

```bash
./EFatoraJoConsoleApp --invoice-file "/invoices/inv001.json" --client-id "ABC123" --secret-key "xyz789secret"
```

### Example 2: Submit Return Invoice

**Windows:**

```powershell
EFatoraJoConsoleApp.exe --return-file "C:\returns\ret001.json" --client-id "ABC123" --secret-key "xyz789secret"
```

**Linux/macOS:**

```bash
./EFatoraJoConsoleApp --return-file "/returns/ret001.json" --client-id "ABC123" --secret-key "xyz789secret"
```

### Example 3: Save Output to File

**Windows:**

```powershell
EFatoraJoConsoleApp.exe --invoice-file invoice.json --client-id "ABC" --secret-key "XYZ" > result.json
```

**Linux/macOS:**

```bash
./EFatoraJoConsoleApp --invoice-file invoice.json --client-id "ABC" --secret-key "XYZ" > result.json
```

### Example 4: Using Environment Variables

```bash
# Set environment variables
export EFATORA_CLIENT_ID="ABC123"
export EFATORA_SECRET_KEY="xyz789secret"

# Use in command
./EFatoraJoConsoleApp --invoice-file invoice.json --client-id "$EFATORA_CLIENT_ID" --secret-key "$EFATORA_SECRET_KEY"
```

## 10. Return Invoices

Return invoices follow a special structure. The input matches the original invoice (positive values), and the application applies negative signs internally for submission.

### Key Differences for Return Invoices

1. **Input Values Remain Positive**: Provide the original invoice values; the converter negates totals/quantities for you
2. **Return Invoice Number**: Must be provided (no auto-generation)
3. **Original Invoice Reference**: `originalInvoiceNumber` is required; UUID link is optional
4. **Return Reason**: Required
5. **Payment Type Sentinel**: `paymentType: "SameAsOriginal"` allowed; requires `originalPaymentType`
6. **Type**: Must be `"SalesReturn"` (literal)

### Return Invoice Example

```json
{
  "originalInvoiceNumber": "INV-2025-001234",
  "returnInvoiceNumber": "RET-INV-2025-001234",
  "returnReason": "Customer request / item returned",
  "returnUUID": "21a105e5-4f52-4bb3-8b8b-38cdbbb6da3c",
  "uniqueSerialNumber": "c3f2a1b9-7a11-4f5f-a1c2-8e0d9f7c1234",
  "invoiceDate": "2025-01-15",
  "paymentType": "SameAsOriginal",
  "originalPaymentType": "LocalGeneralSalesCash",
  "originalInvoiceType": "GeneralSales",
  "type": "SalesReturn",
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
      "taxCategory": "Z",
      "description": "Item Description",
      "quantity": 2,
      "unitPriceBeforeTax": 50.0,
      "totalBeforeTax": 100.0,
      "taxAmount": 16.0,
      "totalIncludingTax": 116.0
    }
  ],
  "invoiceNote": "Optional note carried from original"
}
```

## 11. Output Format

### Success Output

#### Text Mode
```
SUCCESS: Invoice submitted successfully
Invoice Number: INV-20241122-143052
Invoice UUID: 123e4567-e89b-12d3-a456-426614174000
```

#### JSON Mode
```json
{
  "success": true,
  "message": "Invoice submitted successfully",
  "exitCode": 0,
  "alreadySubmitted": null,
  "result": { ... EInvoiceResponse from SDK ... }
}
```

### Error Output

#### Text Mode
```
ERROR (1 - ValidationError): Invoice validation failed
Details:
- InvoiceDate: Invoice date cannot be in the future. Got: 2025-12-31
- Customer.TaxNumber: Required for SpecialSales invoices
```

#### JSON Mode
```json
{
  "success": false,
  "errorType": "ValidationError",
  "message": "Invoice validation failed",
  "exitCode": 1,
  "errors": [
    "Customer name is required",
    "Invoice total must be greater than 0"
  ]
}
```

## 12. Exit Codes

| Code | Name | Description | Required Action |
|------|------|-------------|-----------------|
| `0` | Success | Submission successful | None |
| `1` | ValidationError | Data validation failed | Review error messages and fix data |
| `2` | ApiError | API connection error | Check internet connection and API status |
| `3` | AuthenticationError | Invalid credentials | Verify ClientId and SecretKey |
| `4` | JsonParseError | JSON format error | Review JSON format and fix |
| `5` | ConfigurationError | Missing configuration or invalid parameters | Review command line parameters |
| `6` | FileNotFoundError | File not found | Verify file path |
| `99` | UnexpectedError | Unexpected error | Review logs and contact support |

## 13. Batch Processing Scripts

### Bash Script (Linux/macOS)

```bash
#!/bin/bash

# Configuration
INVOICE_DIR="/data/invoices/pending"
PROCESSED_DIR="/data/invoices/processed"
FAILED_DIR="/data/invoices/failed"
LOG_FILE="/var/log/efatorajo/process.log"
CLIENT_ID="YOUR_CLIENT_ID"
SECRET_KEY="YOUR_SECRET_KEY"

# Process all invoices
for invoice_file in "$INVOICE_DIR"/*.json; do
  if [ -f "$invoice_file" ]; then
    filename=$(basename "$invoice_file")

    echo "[$(date)] Processing: $filename" >> "$LOG_FILE"

    # Submit invoice
    ./EFatoraJoConsoleApp --invoice-file "$invoice_file" --client-id "$CLIENT_ID" --secret-key "$SECRET_KEY" > "/tmp/result_$filename"
    
    exit_code=$?

    if [ $exit_code -eq 0 ]; then
      # Success
      invoice_number=$(cat "/tmp/result_$filename" | jq -r '.result.invoiceNumber')
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
    sleep 1  # Avoid overwhelming the server
  fi
done
```

### PowerShell Script (Windows)

```powershell
# Configuration
$InvoiceDir = "C:\Data\Invoices\Pending"
$ProcessedDir = "C:\Data\Invoices\Processed"
$FailedDir = "C:\Data\Invoices\Failed"
$LogFile = "C:\Logs\EFatoraJo\process.log"
$ClientId = "YOUR_CLIENT_ID"
$SecretKey = "YOUR_SECRET_KEY"

# Process all invoices
Get-ChildItem -Path $InvoiceDir -Filter "*.json" | ForEach-Object {
    $invoiceFile = $_.FullName
    $filename = $_.Name
    $resultFile = Join-Path $env:TEMP "result_$filename"

    Add-Content -Path $LogFile -Value "[$(Get-Date)] Processing: $filename"

    # Submit invoice
    & "C:\EFatoraJo\EFatoraJoConsoleApp.exe" --invoice-file $invoiceFile --client-id $ClientId --secret-key $SecretKey | Out-File $resultFile

    if ($LASTEXITCODE -eq 0) {
        # Success
        $result = Get-Content $resultFile | ConvertFrom-Json
        $invoiceNumber = $result.result.invoiceNumber
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
    Start-Sleep -Seconds 1  # Avoid overwhelming the server
}
```

### Python Script

```python
import subprocess
import json
import sys
import os

def submit_invoice(invoice_file, client_id, secret_key):
    """Submit invoice to Jofotara"""
    try:
        result = subprocess.run(
            [
                "EFatoraJoConsoleApp.exe",
                "--invoice-file", invoice_file,
                "--client-id", client_id,
                "--secret-key", secret_key
            ],
            capture_output=True,
            text=True
        )

        output = json.loads(result.stdout)

        if output['success']:
            print(f" Success! Invoice: {output['result']['invoiceNumber']}")
            return True
        else:
            print(f" Error: {output['message']}")
            for error in output['errors']:
                print(f"  - {error}")
            return False

    except json.JSONDecodeError:
        print(" Failed to parse output")
        print(result.stdout)
        return False

# Usage
if __name__ == "__main__":
    invoice_file = "invoice.json"
    client_id = os.environ.get("EFATORA_CLIENT_ID", "YOUR_CLIENT_ID")
    secret_key = os.environ.get("EFATORA_SECRET_KEY", "YOUR_SECRET_KEY")
    
    success = submit_invoice(invoice_file, client_id, secret_key)
    sys.exit(0 if success else 1)
```

### Node.js Script

```javascript
const { exec } = require('child_process');
const fs = require('fs');

function submitInvoice(invoiceFile, clientId, secretKey) {
    return new Promise((resolve, reject) => {
        const command = `EFatoraJoConsoleApp.exe --invoice-file "${invoiceFile}" --client-id "${clientId}" --secret-key "${secretKey}"`;
        
        exec(command, (error, stdout, stderr) => {
            if (error) {
                reject(error);
                return;
            }
            
            try {
                const result = JSON.parse(stdout);
                resolve(result);
            } catch (parseError) {
                reject(parseError);
            }
        });
    });
}

// Usage
async function processInvoice() {
    try {
        const result = await submitInvoice(
            'invoice.json',
            process.env.EFACTORA_CLIENT_ID || 'YOUR_CLIENT_ID',
            process.env.EFACTORA_SECRET_KEY || 'YOUR_SECRET_KEY'
        );
        
        if (result.success) {
            console.log(` Success! Invoice: ${result.result.invoiceNumber}`);
        } else {
            console.log(` Error: ${result.message}`);
            result.errors.forEach(error => console.log(`  - ${error}`));
        }
    } catch (error) {
        console.error(' Failed to process invoice:', error.message);
    }
}

processInvoice();
```

## 14. Services

### Linux systemd Service

#### Create Service User

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

#### Create Service File

```bash
sudo nano /etc/systemd/system/efatorajo-processor.service
```

**Service File Content:**

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

[Install]
WantedBy=multi-user.target
```

#### Enable and Start Service

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

### macOS LaunchAgent

#### Create LaunchAgent File

```bash
nano ~/Library/LaunchAgents/com.efatorajo.processor.plist
```

**LaunchAgent Content:**

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
</dict>
</plist>
```

#### Load and Start LaunchAgent

```bash
# Load the agent
launchctl load ~/Library/LaunchAgents/com.efatorajo.processor.plist

# Start immediately
launchctl start com.efatorajo.processor

# Check status
launchctl list | grep efatorajo
```

### Windows Service (NSSM)

#### Install NSSM

1. Download from: https://nssm.cc/download
2. Extract to `C:\Tools\nssm\`

#### Install Service

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

# Start service
.\nssm.exe start EFatoraJoProcessor
```

## 15. Security Best Practices

### 1. Credential Protection

-  **Use User Secrets** for storing credentials
-  **Never commit** ClientId and SecretKey to code or Git
-  **Use environment variables** in specific production environments
-  **Rotate credentials** regularly

### 2. Data Validation

-  **Validate JSON format** before submission
-  **Validate dates** (must not be in the future)
-  **Validate numbers** (must be positive for sales, negative for returns)
-  **Validate required fields** based on invoice type

### 3. Error Handling

-  **Log all errors** with full details
-  **Retry** on network errors
-  **Don't retry** on validation errors
-  **Send alerts** on repeated failures

### 4. Performance

-  **Use batch processing** for multiple invoices
-  **Set appropriate timeout** values
-  **Monitor resource usage** (CPU, Memory)
-  **Archive old logs** regularly

### 5. Backup

-  **Keep copies** of all submitted invoices
-  **Keep copies** of results and QR Codes
-  **Backup configuration** regularly
-  **Test data recovery** periodically

## 16. Troubleshooting

### Issue: AuthenticationError (Exit Code 3)

**Symptoms:**
```
 ERROR: Authentication failed - please check your ClientId and SecretKey
Exit Code: 3 (AuthenticationError)
```

**Solutions:**
1. Verify ClientId and SecretKey are correct
2. Ensure account is active on Jofotara
3. Check User Secrets:
   ```bash
   dotnet user-secrets list
   ```

### Issue: JsonParseError (Exit Code 4)

**Symptoms:**
```
 ERROR: Failed to parse invoice JSON
Exit Code: 4 (JsonParseError)

Details:
  - Property 'invoiceDate' must be in yyyy-MM-dd format
```

**Solutions:**
1. Validate JSON using a JSON validator
2. Ensure date format: `yyyy-MM-dd`
3. Ensure correct data types (numbers as numbers, strings as strings)
4. Use `--sample` to display correct format

### Issue: ValidationError (Exit Code 1)

**Symptoms:**
```
 ERROR: Invoice validation failed
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
3. Verify values (not negative for sales, dates correct, etc.)

### Issue: ConfigurationError (Exit Code 5)

**Symptoms:**
```
 ERROR: Required credentials not provided
Exit Code: 5 (ConfigurationError)

Details:
  - --client-id
  - --secret-key
```

**Solutions:**
1. Ensure you provide both `--client-id` and `--secret-key` parameters
2. Do not combine `--invoice-file` and `--return-file` in the same command
3. Use one invoice type per command

## 17. FAQ

### Q: Can I submit both invoice and return invoice in one command?

**A:** No. The application requires you to submit either a sales invoice OR a return invoice, not both in the same command. This is by design to ensure proper processing and error handling.

### Q: Do I need to configure User Secrets for command-line usage?

**A:** No. When using command-line parameters, you provide the `--client-id` and `--secret-key` directly with each command. User Secrets are only used for interactive mode.

### Q: What are the maximum limits for invoice size?

**A:** The application does not impose limits on invoice size, but Jofotara API limits should be considered (typically reasonable for normal invoices).

### Q: How do I handle failed invoices?

**A:**
1. Review error messages in output
2. Fix the issue in the data
3. Resubmit
4. Keep failed invoices in a separate folder for review

### Q: Can I use the application in a Docker environment?

**A:** Yes, you can create a Docker image for the application. Example:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "EFatoraJoConsoleApp.dll"]
```

### Q: Does the application support batch submission?

**A:** Not directly. Use a script for batch processing (see "Batch Processing Scripts" section).

## 18. Version Information

### Version 1.0.1 (Current)

**Features:**
- Submit invoices and return invoices from JSON files
- Support for Income, General Sales, and Special Sales invoice types
- JSON and Text output formats
- Unified exit codes (0-6, 99)
- Strict data validation
- Embedded JSON samples
- Interactive mode for testing
- Cross-platform support

**Changes from 1.0.0:**
- Updated CLI to require `--client-id` and `--secret-key` parameters
- Removed ability to combine invoice and return files in one command
- Enhanced error handling and output formats
- Improved security by not storing credentials in application
- Unified formatter with JSON/text modes and `--output-format`
- Return invoice input schema: explicit fields (`SalesReturn`, `SameAsOriginal`, required numbers), no auto-generated numbers

---

**Release Date:** January 2025
**Last Updated:** January 2025
**Version:** 1.0.1
