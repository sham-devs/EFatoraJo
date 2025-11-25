# Jordan e-Invoicing Developer Reference

This document provides a comprehensive mapping between C# classes, Arabic labels, and UBL 2.1 XML elements for building e-invoicing solutions.

---

## 1. C# Classes & UBL Mappings

This section details the C# classes and their corresponding UBL XML elements for creating e-invoices.

### 1.1. Invoice

This class represents the top-level invoice document.

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|:------------|:----------------|:------------------------|:------|
| **InvoiceNumber** | رقم الفاتورة / رقم فاتورة البائع | `<cbc:ID>` | Mandatory |
| **UniqueSerialNumber** | رقم متسلسل / UUID | `<cbc:UUID>` | Mandatory |
| **InvoiceDate** | تاريخ إصدار الفاتورة / تاريخ الفاتورة | `<cbc:IssueDate>` | Format `yyyy-MM-dd` |
| **PaymentType** | نوع الفاتورة / طريقة الدفع / كود الدفع | `<cbc:InvoiceTypeCode name="...">` | Enum |
| **Type** | نوع الفاتورة / فاتورة ضريبة دخل / فاتورة مبيعات | — | `InvoiceType` enum |
| **Currency** | نوع العملة / عملة الفاتورة | `<cbc:DocumentCurrencyCode>` | Default `JOD` |
| **InvoiceNote** | ملاحظة / وصف الفاتورة / ملاحظات | `<cbc:Note>` | Optional |
| **Supplier** | البيانات الخاصة بالبائع / بيانات المكلف | `<cac:AccountingSupplierParty>` | Object |
| **Customer** | بيانات المشتري | `<cac:AccountingCustomerParty>` | Object |
| **InvoiceTotals** | المدخلات الخاصة بإجمالي الفاتورة | `<cac:LegalMonetaryTotal>` | Object |
| **InvoiceDetails** | المدخلات الخاصة بتفاصيل سلع الفاتورة | `<cac:InvoiceLine>` | List |

### 1.2. Supplier

This class holds the information about the supplier (seller).

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|:------------|:----------------|:------------------------|:------|
| **TaxVATNumber** | الرقم الضريبي للبائع / رقم الضريبة للمكلف | `<cac:PartyTaxScheme><cbc:CompanyID>` | 8 digits |
| **IncomeSourceSequence** | تسلسل مصدر الدخل / تسلسل النشاط للمكلف | `<cac:SellerSupplierParty><cbc:ID>` | Income source |
| **RegisteredSupplierName** | اسم البائع / اسم المكلف | `<cac:PartyLegalEntity><cbc:RegistrationName>` | Mandatory |

### 1.3. Customer

This class contains the details of the customer (buyer).

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|:------------|:----------------|:------------------------|:------|
| **IdentificationNumber** | الرقم الوطني / الرقم الضريبي / الرقم الشخصي | `<cac:PartyIdentification><cbc:ID>` | Depends on `IdentificationType` |
| **IdentificationType** | نوع المعرفات الإضافية / schemeID | Attribute `schemeID="NIN\|PN\|TN"` | Mandatory if `IdentificationNumber` provided |
| **PostalCode** | الرمز البريدي / PostalZone | `<cac:PostalAddress><cbc:PostalZone>` | Optional |
| **Name** | اسم المشتري / RegistrationName | `<cac:PartyLegalEntity><cbc:RegistrationName>` | Mandatory for credit or >100 JOD |
| **PhoneNumber** | رقم الهاتف / Telephone | `<cac:AccountingContact><cbc:Telephone>` | Optional |

### 1.4. InvoiceTotals

This class summarizes the monetary values of the invoice.

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|:------------|:----------------|:------------------------|:------|
| **TotalVATAmount** | مجموع قيمة الضريبة العامة | `<cac:TaxTotal><cbc:TaxAmount>` (VAT block) | When VAT applies |
| **TotalSpecialTaxAmount** | مجموع قيمة الضريبة الخاصة | `<cac:TaxTotal><cbc:TaxAmount>` (Special tax block) | When special tax applies |
| **TotalBeforeDiscount** | إجمالي الفاتورة قبل الخصم | `<cac:LegalMonetaryTotal><cbc:TaxExclusiveAmount>` | Before discounts & taxes |
| **TotalInvoiceAmount** | إجمالي الفاتورة | `<cac:LegalMonetaryTotal><cbc:TaxInclusiveAmount>` | After tax & discount |
| **TotalDiscountAmount** | مجموع قيمة الخصم | `<cac:LegalMonetaryTotal><cbc:AllowanceTotalAmount>` | Sum of line discounts |
| **FinalPayableAmount** | إجمالي الفاتورة | `<cac:LegalMonetaryTotal><cbc:PayableAmount>` | Net amount to be paid |

### 1.5. InvoiceDetail

This class represents a single line item or service within the invoice.

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|:------------|:----------------|:------------------------|:------|
| **ID** | رقم تسلسلي للسلعة / رقم تسلسلي | `<cac:InvoiceLine><cbc:ID>` | Sequential inside invoice |
| **Description** | وصف السلعة او الخدمة / Name | `<cac:Item><cbc:Name>` | Mandatory |
| **Quantity** | الكمية / InvoicedQuantity | `<cbc:InvoicedQuantity unitCode="PCE">` | Integer or decimal |
| **UnitPriceBeforeTax** | سعر الوحدة قبل الضريبة / PriceAmount | `<cac:Price><cbc:PriceAmount>` | Unit price excluding tax |
| **TotalBeforeTax** | المبلغ الإجمالي قبل الضريبة / LineExtensionAmount | `<cbc:LineExtensionAmount>` | Qty × Price – Discount |
| **DiscountAmount** | قيمة الخصم / خصم السلعة | `<cac:AllowanceCharge><cbc:Amount>` | Per line |
| **TaxAmount** | قيمة الضريبة / TaxAmount | `<cac:TaxTotal><cbc:TaxAmount>` | Calculated per line |
| **TotalIncludingTax** | المبلغ الإجمالي شامل الضريبة / RoundingAmount | `<cbc:RoundingAmount>` | After tax |
| **TaxRate** | نسبة الضريبة / Percent | `<cbc:Percent>` | 0,1,2,4,5,7,8,10,16 % |
| **SpecialTaxAmount** | قيمة الضريبة الخاصة | `<cac:TaxTotal><cbc:TaxAmount>` | Only for special tax items |
| **UnitCode** | وحدة الكمية | Attribute `unitCode="PCE"` | Default "PCE" |
| **TaxCategoryId** | معرف فئة الضريبة | `<cbc:ID schemeAgencyID="6" schemeID="UN/ECE 5305">` | "S", "Z", "O" |
| **TaxSchemeId** | معرف نظام الضريبة | `<cbc:ID schemeAgencyID="6" schemeID="UN/ECE 5153">` | "VAT", "OTH" |
| **BaseQuantity** | الكمية الأساسية | `<cbc:BaseQuantity unitCode="C62">` | Default 1 |

### 1.6. Enum Documentation

| Enum Name | Arabic Name | Description |
|:---|:---|:---|
| **CountrySubentityCode** | المحافظات | List of Jordanian governorates. |
| **CurrencyCode** | العملات | Supported invoice currencies. |
| **IdentificationType** | نوع المعرف | Identifiers for the customer. |
| **InvoicePaymentTypeCode** | كود طريقة الدفع | Categorized payment methods. |
| **InvoiceType** | نوع الفاتورة | Business meaning of the invoice. |
| **TaxCategoryCode** | فئة الضريبة | VAT tax rates. |

---

## 2. JoFotara Response Classes & Enums

This section details the C# classes and enums used to parse the **JSON response** from the JoFotara API. No Arabic labels are included as they are not provided in the API response.

### 2.1. Response Classes

This section provides the structure of the JSON response objects.

#### EInvoiceResponse – Root Object

This is the top-level object returned by the API.

| C# Property | JoFotara JSON Key | Data Type | Notes |
|:------------|:------------------|:----------|:------|
| **Results** | `"EINV_RESULTS"` | `EInvoiceResults` | Detailed processing result. |
| **Status** | `"EINV_STATUS"` | `EInvoiceStatus` | Overall submission status. |
| **SignedInvoice** | `"EINV_SINGED_INVOICE"` | `string?` | Base64 signed XML (nullable). |
| **Qr** | `"EINV_QR"` | `string?` | QR code string (nullable). |
| **InvoiceNumber** | `"EINV_NUM"` | `string?` | JoFotara invoice number (nullable). |
| **InvoiceUuid** | `"EINV_INV_UUID"` | `string?` | Global UUID (nullable). |

#### EInvoiceResults – Processing Details

This object contains detailed feedback from the server after processing the invoice.

| C# Property | JoFotara JSON Key | Data Type | Notes |
|:------------|:------------------|:----------|:------|
| **Status** | `"status"` | `EInvoiceProcessingStatus` | `PASS` or `ERROR`. |
| **Info** | `"INFO"` | `List<EInvoiceMessage>` | Non-blocking info messages. |
| **Warnings** | `"WARNINGS"` | `List<EInvoiceMessage>` | Warning messages. |
| **Errors** | `"ERRORS"` | `List<EInvoiceMessage>` | Blocking error messages. |

#### EInvoiceMessage – Single Message Item

This object represents a single message from the API.

| C# Property | JoFotara JSON Key | Data Type | Notes |
|:------------|:------------------|:----------|:------|
| **Type** | `"type"` | `EInvoiceMessageType` | `INFO`, `WARNING`, `ERROR`. |
| **Status** | `"status"` | `EInvoiceMessageStatus` | `PASS`, `WARNING`, `ERROR`. |
| **Code** | `"EINV_CODE"` | `string?` | JoFotara internal code. |
| **Category** | `"EINV_CATEGORY"` | `string?` | High-level category. |
| **Message** | `"EINV_MESSAGE"` | `string?` | Human-readable message. |

### 2.2. Enum Values

This section defines the possible values for the enums in the response classes.

#### EInvoiceStatus – Overall Submission State

This enum indicates the final state of the invoice submission.

| Enum Value | JoFotara Value | Description |
|:---|:---|:---|
| **NOT_SUBMITTED** | `"NOT_SUBMITTED"` | Invoice has **not** been sent. |
| **SUBMITTED** | `"SUBMITTED"` | Successfully received. |
| **REJECTED** | `"REJECTED"` | Rejected due to validation errors. |
| **ACCEPTED** | `"ACCEPTED"` | Stored and accepted. |
| **ALREADY_SUBMITTED** | `"ALREADY_SUBMITTED"` | Duplicate submission. |

#### EInvoiceProcessingStatus – Result Status

This enum indicates the result of the server-side processing.

| Enum Value | JoFotara Value | Description |
|:---|:---|:---|
| **PASS** | `"PASS"` | Processing succeeded. |
| **ERROR** | `"ERROR"` | Processing failed. |

#### EInvoiceMessageType – Message Category

This enum classifies the type of message.

| Enum Value | JoFotara Value | Description |
|:---|:---|:---|
| **INFO** | `"INFO"` | Informational message. |
| **WARNING** | `"WARNING"` | Warning message. |
| **ERROR** | `"ERROR"` | Blocking error message. |

#### EInvoiceMessageStatus – Message State

This enum indicates the status of a specific message.

| Enum Value | JoFotara Value | Description |
|:---|:---|:---|
| **PASS** | `"PASS"` | Indicates success. |
| **WARNING** | `"WARNING"` | Indicates warning. |
| **ERROR** | `"ERROR"` | Indicates error. |
