# EFatoraJo Error Handling Guide

## Overview

This guide provides comprehensive information about error handling in the EFatoraJo SDK and Console Application, including error types, validation rules, recovery strategies, and best practices.

---

## Error Types and Hierarchy

### 1. SDK Exceptions

| Exception Type | Description | Common Causes | Recovery Strategy |
|----------------|-------------|----------------|------------------|
| `InvoiceValidationException` | Invoice data validation failed | Missing required fields, invalid formats, business rule violations | Fix data issues and resubmit |
| `UblGenerationException` | UBL document creation failed | Malformed data, XML generation issues | Check data structure and retry |
| `EInvoiceApiException` | API communication error | Network issues, server errors, invalid credentials | Check connection, verify credentials, retry if appropriate |
| `EInvoiceSerializationException` | Serialization/deserialization failed | Invalid XML/JSON, encoding issues | Check data format and encoding |
| `EInvoiceException` | Base exception for other errors | Unexpected system errors | Log and contact support |

### 2. Console Application Exit Codes

| Exit Code | Name | Description | Action Required |
|-----------|-------|-------------|------------------|
| `0` | Success | Invoice submitted successfully | None |
| `1` | ValidationError | Data validation failed | Review error messages and fix data |
| `2` | ApiError | API connection error | Check internet connection and API status |
| `3` | AuthenticationError | Invalid credentials | Verify ClientId and SecretKey |
| `4` | JsonParseError | JSON format error | Review JSON format and fix |
| `5` | ConfigurationError | Missing configuration | Review User Secrets setup |
| `6` | FileNotFoundError | File not found | Verify file path |
| `99` | UnexpectedError | Unexpected error | Review logs and contact support |

---

## Validation Rules and Error Messages

### Invoice Validation Rules

#### Basic Details Validation

| Field | Validation Rule | Error Message | Solution |
|--------|-----------------|---------------|-----------|
| `InvoiceNumber` | Required, non-empty, unique | "InvoiceNumber is required" | Provide unique invoice number |
| `InvoiceDate` | Required, yyyy-MM-dd format, not future | "InvoiceDate must be in valid yyyy-MM-dd format" / "InvoiceDate cannot be in the future" | Use correct format and valid date |
| `PaymentType` | Required, valid enum value | "PaymentType is invalid" | Use valid payment type |
| `Currency` | Valid currency code (JOD, USD, EUR) | "Invalid currency code" | Use supported currency codes |

#### Supplier Validation

| Field | Validation Rule | Error Message | Solution |
|--------|-----------------|---------------|-----------|
| `TaxVATNumber` | Required, 8 digits | "Supplier TaxVATNumber is required" | Provide valid 8-digit tax number |
| `RegisteredSupplierName` | Required, non-empty | "Supplier RegisteredSupplierName is required" | Provide supplier name |
| `IncomeSourceSequence` | Required, valid format | "Supplier IncomeSourceSequence is required" | Provide activity code |

#### Customer Validation

| Field | Validation Rule | Error Message | Solution |
|--------|-----------------|---------------|-----------|
| `Name` | Required, non-empty | "Customer Name is required" | Provide customer name |
| `IdentificationNumber` | Required for credit invoices >100 JOD | "Customer IdentificationNumber is required" | Provide ID number |
| `IdentificationType` | Valid enum value (NIN, PN, TN) | "Invalid identification type" | Use valid identification type |
| `PhoneNumber` | Format: +962XXXXXXXXX | "Invalid phone number format" | Use Jordanian format with country code |

#### Invoice Line Validation

| Field | Validation Rule | Error Message | Solution |
|--------|-----------------|---------------|-----------|
| `ID` | Required, unique within invoice | "InvoiceDetail ID is required" | Provide unique line ID |
| `Description` | Required, non-empty | "Description is required" | Provide item description |
| `Quantity` | Required, > 0 | "Quantity must be greater than zero" | Provide positive quantity |
| `UnitPriceBeforeTax` | Required, > 0 | "UnitPriceBeforeTax must be greater than zero" | Provide positive price |
| `TaxCategory` | Valid enum value | "Invalid TaxCategory value" | Use valid tax category |
| `TaxRate` | Must match TaxCategory | "Invalid TaxRate for line" | Use correct tax rate for category |
| `DiscountAmount` | Optional, >= 0 | "DiscountAmount cannot be negative" | Use non-negative discount |

#### Invoice Totals Validation

| Field | Validation Rule | Error Message | Solution |
|--------|-----------------|---------------|-----------|
| `TotalBeforeDiscount` | Must equal sum of (quantity × price) | "Invalid TotalBeforeDiscount" | Recalculate based on line items |
| `TotalVATAmount` | Must match calculated VAT | "Invalid TotalVATAmount" | Recalculate VAT amount |
| `TotalInvoiceAmount` | Must equal base + taxes - discounts | "Invalid TotalInvoiceAmount" | Recalculate total amount |
| `FinalPayableAmount` | Must equal TotalInvoiceAmount | "FinalPayableAmount must equal TotalInvoiceAmount" | Ensure amounts match |

### Invoice Type Specific Validation

#### Income Invoice (Type: Income)
| Rule | Validation | Error Message | Solution |
|-------|-------------|---------------|-----------|
| No VAT | TaxAmount must be 0 | "Income invoice: line X TaxAmount must be 0" | Set TaxAmount to 0 |
| No Special Tax | SpecialTaxAmount must be 0 | "Income invoice: line X SpecialTaxAmount must be 0" | Set SpecialTaxAmount to 0 |
| Tax Category | Must be Z (Zero-rated) | "Invalid tax category for Income invoice" | Use TaxCategoryCode.Z |

#### General Sales Invoice (Type: GeneralSales)
| Rule | Validation | Error Message | Solution |
|-------|-------------|---------------|-----------|
| VAT Required | TaxAmount > 0 | "GeneralSales invoice: line X must have VAT" | Calculate and include VAT |
| No Special Tax | SpecialTaxAmount must be 0 | "GeneralSales invoice: line X must not have SpecialTaxAmount" | Remove special tax |
| Tax Category | Can be S, S8, S5, O | "Invalid tax category for GeneralSales" | Use appropriate tax category |

#### Special Sales Invoice (Type: SpecialSales)
| Rule | Validation | Error Message | Solution |
|-------|-------------|---------------|-----------|
| VAT Required | TaxAmount > 0 | "SpecialSales invoice: line X must have VAT" | Calculate and include VAT |
| Special Tax Required | SpecialTaxAmount > 0 | "SpecialSales invoice: line X SpecialTaxAmount is required" | Calculate and include special tax |
| Customer Tax Number | Required for all SpecialSales | "Customer.TaxNumber: Required for SpecialSales invoices" | Provide customer tax number |
| Tax Category | Can be S, S8, S5 | "Invalid tax category for SpecialSales" | Use special tax categories |

---

## API Error Responses

### Response Structure

```json
{
  "EINV_RESULTS": {
    "status": "PASS" | "ERROR",
    "INFO": [...],
    "WARNINGS": [...],
    "ERRORS": [...]
  },
  "EINV_STATUS": "SUBMITTED" | "REJECTED" | "ACCEPTED" | "ALREADY_SUBMITTED",
  "EINV_QR": "base64-encoded-qr-code",
  "EINV_NUM": "system-generated-number",
  "EINV_INV_UUID": "unique-identifier"
}
```

### Common API Errors

| Error Code | Message | Cause | Solution |
|------------|----------|--------|----------|
| `AUTH_FAILED` | Authentication failed | Invalid ClientId/SecretKey | Verify credentials |
| `INVALID_INVOICE` | Invoice validation failed | Data validation errors on server | Fix validation issues |
| `DUPLICATE_INVOICE` | Invoice already submitted | Duplicate invoice number | Use unique invoice number |
| `SYSTEM_ERROR` | Internal server error | Temporary server issue | Retry after delay |
| `RATE_LIMITED` | Too many requests | Rate limiting exceeded | Implement backoff strategy |
| `NETWORK_ERROR` | Connection failed | Network connectivity issues | Check network, retry |

---

## Error Handling Best Practices

### 1. Structured Error Handling

```csharp
try
{
    var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
    
    if (response.IsSuccessfullySubmitted())
    {
        // Handle success
        logger.LogInformation("Invoice {InvoiceNumber} submitted successfully", response.InvoiceNumber);
        return new SubmissionResult { Success = true, InvoiceNumber = response.InvoiceNumber };
    }
    else
    {
        // Handle API validation errors
        var errors = response.GetFormattedErrors();
        logger.LogWarning("Invoice submission failed: {Errors}", errors);
        return new SubmissionResult { Success = false, Errors = errors };
    }
}
catch (InvoiceValidationException ex)
{
    // Handle validation errors
    logger.LogWarning("Invoice validation failed: {Errors}", 
        string.Join(", ", ex.ValidationErrors));
    return new SubmissionResult { Success = false, Errors = ex.ValidationErrors };
}
catch (EInvoiceApiException ex)
{
    // Handle API errors
    logger.LogError(ex, "JoFatora API error: {StatusCode}", ex.StatusCode);
    
    if (ex.IsRetryable)
    {
        // Implement retry logic
        return await RetrySubmissionAsync(invoice, clientId, secretKey);
    }
    
    return new SubmissionResult { Success = false, Error = ex.Message };
}
catch (Exception ex)
{
    // Handle unexpected errors
    logger.LogError(ex, "Unexpected error during invoice submission");
    return new SubmissionResult { Success = false, Error = "Internal error occurred" };
}
```

### 2. Retry Logic Implementation

```csharp
private async Task<SubmissionResult> RetrySubmissionAsync(
    Invoice invoice, string clientId, string secretKey, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
            return new SubmissionResult { Success = true, Response = response };
        }
        catch (EInvoiceApiException ex) when (ex.IsRetryable && attempt < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
            logger.LogWarning("Attempt {Attempt} failed, retrying in {Delay}s", attempt, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
    
    return new SubmissionResult { Success = false, Error = "Max retries exceeded" };
}
```

### 3. Logging Strategy

```csharp
// Structured logging with context
logger.LogInformation("Processing invoice {InvoiceNumber} for customer {CustomerName}", 
    invoice.InvoiceNumber, invoice.Customer.Name);

logger.LogWarning("Validation failed for invoice {InvoiceNumber}: {Errors}", 
    invoice.InvoiceNumber, string.Join(", ", validationErrors));

logger.LogError(ex, "API call failed for invoice {InvoiceNumber}", invoice.InvoiceNumber);

// Include correlation IDs for tracking
var correlationId = Guid.NewGuid().ToString();
using (logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["InvoiceNumber"] = invoice.InvoiceNumber
}))
{
    // Process invoice
}
```

### 4. User-Friendly Error Messages

```csharp
public static string GetUserFriendlyMessage(InvoiceValidationException ex)
{
    var messageMap = new Dictionary<string, string>
    {
        ["InvoiceDate cannot be in the future"] = "تاريخ الفاتورة لا يمكن أن يكون في المستقبل",
        ["Customer TaxNumber: Required for SpecialSales invoices"] = "الرقم الضريبي للمشتري مطلوب لفواتير المبيعات الخاصة",
        ["Quantity must be greater than zero"] = "الكمية يجب أن تكون أكبر من صفر",
        ["Invalid TotalInvoiceAmount"] = "المبلغ الإجمالي للفاتورة غير صحيح"
    };
    
    foreach (var error in ex.ValidationErrors)
    {
        if (messageMap.TryGetValue(error, out var friendlyMessage))
        {
            return friendlyMessage;
        }
    }
    
    return "حدث خطأ في التحقق من الفاتورة";
}
```

---

## Console Application Error Handling

### Input Validation

The console application performs two-tier validation:

1. **JSON Schema Validation** - Ensures valid JSON structure
2. **Business Logic Validation** - Validates invoice data rules

### Error Output Formats

#### Text Format
```
✗ ERROR: Invoice validation failed
  Exit Code: 1 (ValidationError)

Details:
  - InvoiceDate: Invoice date cannot be in the future. Got: 2025-12-31
  - Customer.TaxNumber: Required for SpecialSales invoices
```

#### JSON Format
```json
{
  "success": false,
  "errorType": "ValidationError",
  "message": "Invoice validation failed",
  "errors": [
    {
      "field": "InvoiceDate",
      "message": "InvoiceDate cannot be in the future",
      "expectedValue": "Current or past date",
      "actualValue": "2025-12-31"
    }
  ],
  "exitCode": 1
}
```

---

## Troubleshooting Common Issues

### 1. Authentication Failures

**Symptoms:**
- Exit Code 3 (AuthenticationError)
- "Authentication failed - please check your ClientId and SecretKey"

**Troubleshooting Steps:**
1. Verify ClientId and SecretKey are correct
2. Check for leading/trailing spaces in credentials
3. Ensure account is active on Jofotara
4. Verify user secrets are properly configured

### 2. Validation Failures

**Symptoms:**
- Exit Code 1 (ValidationError)
- Detailed validation error messages

**Common Issues and Solutions:**

| Issue | Solution |
|--------|----------|
| Future dates | Use current or past dates only |
| Invalid phone format | Use format: +962XXXXXXXXX |
| Missing tax number | Provide customer tax number for SpecialSales |
| Incorrect totals | Recalculate based on line items |
| Invalid tax categories | Use appropriate categories for invoice type |

### 3. Network Issues

**Symptoms:**
- Exit Code 2 (ApiError)
- Timeout errors
- Connection refused

**Solutions:**
1. Check internet connectivity
2. Verify firewall settings allow HTTPS to jofotara.gov.jo
3. Check proxy settings if applicable
4. Implement retry logic with exponential backoff

### 4. JSON Parsing Issues

**Symptoms:**
- Exit Code 4 (JsonParseError)
- "Failed to parse invoice JSON"

**Common JSON Issues:**
- Missing commas
- Unmatched brackets
- Invalid escape characters
- Incorrect data types

**Solutions:**
1. Use JSON validator (jsonlint.com)
2. Check sample formats with `--sample` option
3. Ensure proper encoding (UTF-8)

---

## Monitoring and Alerting

### Key Metrics to Monitor

1. **Success Rate**: Percentage of successful submissions
2. **Error Distribution**: Breakdown of error types
3. **Response Time**: Average API response time
4. **Retry Rate**: Percentage of submissions requiring retries

### Alert Thresholds

| Metric | Threshold | Alert Level |
|---------|------------|--------------|
| Success Rate | < 95% | Critical |
| Error Rate | > 5% | Warning |
| Response Time | > 30 seconds | Warning |
| Authentication Failures | > 3 in 5 minutes | Critical |

### Log Analysis

```bash
# Analyze error patterns
grep "ERROR" /var/log/efatorajo/process.log | awk '{print $NF}' | sort | uniq -c | sort -nr

# Calculate success rate
total=$(grep -c "Processing" /var/log/efatorajo/process.log)
success=$(grep -c "SUCCESS" /var/log/efatorajo/process.log)
success_rate=$(echo "scale=2; $success * 100 / $total" | bc)
echo "Success Rate: $success_rate%"

# Monitor response times
grep "Invoice submitted" /var/log/efatorajo/process.log | grep -o "[0-9]*ms" | awk '{sum+=$1; count++} END {print "Average:", sum/count, "ms"}'
```

---

## Recovery Procedures

### 1. Data Recovery

1. **Backup Failed Invoices**: Keep copies of failed invoices for manual review
2. **Audit Trail**: Maintain logs of all submission attempts
3. **Data Integrity**: Verify invoice data before resubmission

### 2. Service Recovery

1. **Automatic Restart**: Configure service to restart on failure
2. **Health Checks**: Implement periodic health checks
3. **Fallback Mechanism**: Have manual submission process ready

### 3. Disaster Recovery

1. **Backup Configuration**: Store configuration securely
2. **Documentation**: Keep updated documentation
3. **Contact Information**: Maintain support contact information

---

## Conclusion

Proper error handling is crucial for reliable invoice processing. This guide provides:

- ✅ **Comprehensive error classification** for all error types
- ✅ **Detailed validation rules** with specific error messages
- ✅ **Recovery strategies** for different error scenarios
- ✅ **Best practices** for robust error handling
- ✅ **Monitoring guidelines** for proactive error detection
- ✅ **Troubleshooting procedures** for common issues

Implement these practices to ensure reliable and resilient invoice processing with the EFatoraJo system.