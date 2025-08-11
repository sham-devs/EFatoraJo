# Jordan e-Invoicing Developer Reference

A full mapping table between C# classes, **Jordan e-Invoicing Arabic labels**, and **UBL 2.1 XML elements** is available below.

---

## 1. Invoice

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|-------------|-----------------|-------------------------|-------|
| InvoiceNumber | رقم الفاتورة / رقم فاتورة البائع | `<cbc:ID>` | Mandatory |
| UniqueSerialNumber | رقم متسلسل / UUID | `<cbc:UUID>` | Mandatory |
| InvoiceDate | تاريخ إصدار الفاتورة / تاريخ الفاتورة | `<cbc:IssueDate>` | Format yyyy-MM-dd |
| PaymentType | نوع الفاتورة / طريقة الدفع / كود الدفع | `<cbc:InvoiceTypeCode name="...">` | Enum |
| Type | نوع الفاتورة / فاتورة ضريبة دخل / فاتورة مبيعات | — | InvoiceType enum |
| Currency | نوع العملة / عملة الفاتورة | `<cbc:DocumentCurrencyCode>` | Default JOD |
| InvoiceNote | ملاحظة / وصف الفاتورة / ملاحظات | `<cbc:Note>` | Optional |
| Supplier | البيانات الخاصة بالبائع / بيانات المكلف | `<cac:AccountingSupplierParty>` | Object |
| Customer | بيانات المشتري | `<cac:AccountingCustomerParty>` | Object |
| InvoiceTotals | المدخلات الخاصة بإجمالي الفاتورة | `<cac:LegalMonetaryTotal>` | Object |
| InvoiceDetails | المدخلات الخاصة بتفاصيل سلع الفاتورة | `<cac:InvoiceLine>` | List |

---

## 2. Supplier

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|-------------|-----------------|-------------------------|-------|
| TaxVATNumber | الرقم الضريبي للبائع / رقم الضريبة للمكلف | `<cac:PartyTaxScheme><cbc:CompanyID>` | 8 digits |
| IncomeSourceSequence | تسلسل مصدر الدخل / تسلسل النشاط للمكلف | `<cac:SellerSupplierParty><cbc:ID>` | Income source |
| RegisteredSupplierName | اسم البائع / اسم المكلف | `<cac:PartyLegalEntity><cbc:RegistrationName>` | Mandatory |

---

## 3. Customer

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|-------------|-----------------|-------------------------|-------|
| IdentificationNumber | الرقم الوطني / الرقم الضريبي / الرقم الشخصي | `<cac:PartyIdentification><cbc:ID>` | Depends on IdentificationType |
| IdentificationType | نوع المعرفات الإضافية / schemeID | Attribute schemeID="NIN\|PN\|TN" | Mandatory if IdentificationNumber provided |
| PostalCode | الرمز البريدي / PostalZone | `<cac:PostalAddress><cbc:PostalZone>` | Optional |
| Name | اسم المشتري / RegistrationName | `<cac:PartyLegalEntity><cbc:RegistrationName>` | Mandatory for credit or >100 JOD |
| PhoneNumber | رقم الهاتف / Telephone | `<cac:AccountingContact><cbc:Telephone>` | Optional |

---

## 4. InvoiceTotals

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|-------------|-----------------|-------------------------|-------|
| TotalVATAmount | مجموع قيمة الضريبة العامة | `<cac:TaxTotal><cbc:TaxAmount>` (VAT block) | When VAT applies |
| TotalSpecialTaxAmount | مجموع قيمة الضريبة الخاصة | `<cac:TaxTotal><cbc:TaxAmount>` (Special tax block) | When special tax applies |
| TotalBeforeDiscount | إجمالي الفاتورة قبل الخصم | `<cac:LegalMonetaryTotal><cbc:TaxExclusiveAmount>` | Before discounts & taxes |
| TotalInvoiceAmount | إجمالي الفاتورة | `<cac:LegalMonetaryTotal><cbc:TaxInclusiveAmount>` | After tax & discount |
| TotalDiscountAmount | مجموع قيمة الخصم | `<cac:LegalMonetaryTotal><cbc:AllowanceTotalAmount>` | Sum of line discounts |
| FinalPayableAmount | إجمالي الفاتورة | `<cac:LegalMonetaryTotal><cbc:PayableAmount>` | Net amount to be paid |

---

## 5. InvoiceDetail

| C# Property | Arabic Label(s) | UBL XML Element / Usage | Notes |
|-------------|-----------------|-------------------------|-------|
| ID | رقم تسلسلي للسلعة / رقم تسلسلي | `<cac:InvoiceLine><cbc:ID>` | Sequential inside invoice |
| Description | وصف السلعة او الخدمة / Name | `<cac:Item><cbc:Name>` | Mandatory |
| Quantity | الكمية / InvoicedQuantity | `<cbc:InvoicedQuantity unitCode="PCE">` | Integer or decimal |
| UnitPriceBeforeTax | سعر الوحدة قبل الضريبة / PriceAmount | `<cac:Price><cbc:PriceAmount>` | Unit price excluding tax |
| TotalBeforeTax | المبلغ الإجمالي قبل الضريبة / LineExtensionAmount | `<cbc:LineExtensionAmount>` | Qty × Price – Discount |
| DiscountAmount | قيمة الخصم / خصم السلعة | `<cac:AllowanceCharge><cbc:Amount>` | Per line |
| TaxAmount | قيمة الضريبة / TaxAmount | `<cac:TaxTotal><cbc:TaxAmount>` | Calculated per line |
| TotalIncludingTax | المبلغ الإجمالي شامل الضريبة / RoundingAmount | `<cbc:RoundingAmount>` | After tax |
| TaxRate | نسبة الضريبة / Percent | `<cbc:Percent>` | 0,1,2,4,5,7,8,10,16 % |
| SpecialTaxAmount | قيمة الضريبة الخاصة | `<cac:TaxTotal><cbc:TaxAmount>` | Only for special tax items |
| UnitCode | وحدة الكمية | Attribute unitCode="PCE" | Default "PCE" |
| TaxCategoryId | معرف فئة الضريبة | `<cbc:ID schemeAgencyID="6" schemeID="UN/ECE 5305">` | "S", "Z", "O" |
| TaxSchemeId | معرف نظام الضريبة | `<cbc:ID schemeAgencyID="6" schemeID="UN/ECE 5153">` | "VAT", "OTH" |
| BaseQuantity | الكمية الأساسية | `<cbc:BaseQuantity unitCode="C62">` | Default 1 |

---

## 6. Enum Documentation

### 6.1 CountrySubentityCode (المحافظات)

| Enum Member | Arabic Name | JoFotara Code |
|-------------|-------------|---------------|
| Amman | عمان | JO-AM |
| Zarqa | الزرقاء | JO-AZ |
| Irbid | إربد | JO-IR |
| Aqaba | العقبة | JO-AQ |
| Mafraq | المفرق | JO-MA |
| Jerash | جرش | JO-JA |
| Madaba | مادبا | JO-MD |
| Ajloun | عجلون | JO-AJ |
| Karak | الكرك | JO-KA |
| Tafilah | الطفيلة | JO-AT |
| Maan | معان | JO-MN |
| Balqa | البلقاء | JO-BA |

### 6.2 CurrencyCode (العملات)

| Enum Member | Arabic Name | JoFotara Code |
|-------------|-------------|---------------|
| JOD | دينار أردني | JOD |
| USD | دولار أمريكي | USD |
| EUR | يورو | EUR |
| SAR | ريال سعودي | SAR |
| AED | درهم إماراتي | AED |
| OMR | ريال عماني | OMR |
| GBP | جنيه إسترليني | GBP |
| QAR | ريال قطري | QAR |
| KWD | دينار كويتي | KWD |
| BHD | دينار بحريني | BHD |
| AUD | دولار أسترالي | AUD |
| CAD | دولار كندي | CAD |
| JPY | ين ياباني | JPY |
| CHF | فرنك سويسري | CHF |
| TRY | ليرة تركية | TRY |
| SYP | ليرة سورية | SYP |
| EGP | جنيه مصري | EGP |

### 6.3 IdentificationType (نوع المعرف)

| Enum Member | Arabic Name | JoFotara schemeID |
|-------------|-------------|-------------------|
| NIN | الرقم الوطني | NIN |
| PN | الرقم الشخصي | PN |
| TN | الرقم الضريبي للمشتري | TN |

### 6.4 InvoicePaymentTypeCode (كود طريقة الدفع)

| Enum Member | Arabic Name | JoFotara Code |
|-------------|-------------|---------------|
| LocalIncomeCash | دخل محلي نقدي | 011 |
| LocalIncomeCredit | دخل محلي ذمم | 021 |
| ExportIncomeCash | دخل تصدير نقدي | 111 |
| ExportIncomeCredit | دخل تصدير ذمم | 121 |
| LocalGeneralSalesCash | مبيعات عامة محلية نقدية | 012 |
| LocalGeneralSalesCredit | مبيعات عامة محلية ذمم | 022 |
| ExportGeneralSalesCash | مبيعات عامة تصدير نقدية | 112 |
| ExportGeneralSalesCredit | مبيعات عامة تصدير ذمم | 122 |
| DevelopmentAreaGeneralSalesCash | مبيعات عامة مناطق تنموية نقدية | 212 |
| DevelopmentAreaGeneralSalesCredit | مبيعات عامة مناطق تنموية ذمم | 222 |
| LocalSpecialSalesCash | مبيعات خاصة محلية نقدية | 013 |
| LocalSpecialSalesCredit | مبيعات خاصة محلية ذمم | 023 |
| ExportSpecialSalesCash | مبيعات خاصة تصدير نقدية | 113 |
| ExportSpecialSalesCredit | مبيعات خاصة تصدير ذمم | 123 |
| DevelopmentAreaSpecialSalesCash | مبيعات خاصة مناطق تنموية نقدية | 213 |
| DevelopmentAreaSpecialSalesCredit | مبيعات خاصة مناطق تنموية ذمم | 223 |

### 6.5 InvoiceType (نوع الفاتورة)

| Enum Member | Arabic Name | Business Meaning |
|-------------|-------------|------------------|
| Income | فاتورة دخل | بدون ضريبة |
| GeneralSales | فاتورة مبيعات عامة | ضريبة VAT فقط |
| SpecialSales | فاتورة مبيعات خاصة | VAT + OTH |

### 6.6 TaxCategoryCode (فئة الضريبة)

| Enum Member | Arabic Name | JoFotara Rate |
|-------------|-------------|---------------|
| S | ضريبة عادية 16 % | 16 % |
| O | معفاة (0 %) | 0 % |
| Z | صفرية (0 %) | 0 % |
| S1 | ضريبة مخفضة 1 % | 1 % |
| S2 | ضريبة مخفضة 2 % | 2 % |
| S3 | ضريبة مخفضة 3 % | 3 % |
| S4 | ضريبة مخفضة 4 % | 4 % |
| S5 | ضريبة مخفضة 5 % | 5 % |
| S7 | ضريبة مخفضة 7 % | 7 % |
| S8 | ضريبة مخفضة 8 % | 8 % |
| S10 | ضريبة مخفضة 10 % | 10 % |

# Developer Reference – JoFotara Response Classes & Enums

This section documents every **response field** returned by the Jordan JoFotara API, its **Arabic label** (when provided), the **C# property**, and the **enum values** that may appear.

---

## 1. EInvoiceResponse – Root Response Object

| C# Property | Arabic Label (API Docs) | JSON Key | Purpose / Notes |
|-------------|-------------------------|----------|-----------------|
| **Results** | نتائج الفاتورة | `"EINV_RESULTS"` | Contains lists of info, warnings, errors. |
| **Status** | حالة الفاتورة | `"EINV_STATUS"` | Enum `EInvoiceStatus`. |
| **SignedInvoice** | الفاتورة الموقعة | `"EINV_SINGED_INVOICE"` | Base64-encoded signed XML (nullable). |
| **Qr** | رمز QR للفاتورة | `"EINV_QR"` | Scannable QR code string (nullable). |
| **InvoiceNumber** | رقم الفاتورة | `"EINV_NUM"` | The sequential invoice number assigned by JoFotara. |
| **InvoiceUuid** | UUID الفاتورة | `"EINV_INV_UUID"` | Global unique identifier for the invoice. |

### Helper Methods & Status Checks
| Method | Arabic Description | Usage |
|--------|--------------------|-------|
| `IsSuccessfullySubmitted()` | تم إرسال الفاتورة بنجاح | `Status == SUBMITTED && Results.Status == PASS` |
| `IsAlreadySubmitted()` | الفاتورة مرسلة مسبقًا | `Status == ALREADY_SUBMITTED` |
| `HasNewUuid()` | تم توليد UUID جديد | `!string.IsNullOrEmpty(InvoiceUuid)` |
| `HasErrors()` | توجد أخطاء | `Results.Errors.Count > 0` |
| `HasWarnings()` | توجد تحذيرات | `Results.Warnings.Count > 0` |

---

## 2. EInvoiceResults – Detailed Messages Container

| C# Property | Arabic Label (API Docs) | JSON Key | Purpose / Notes |
|-------------|-------------------------|----------|-----------------|
| **Status** | حالة المعالجة | `"status"` | Enum `EInvoiceProcessingStatus`. |
| **Info** | معلومات | `"INFO"` | List of non-blocking info messages. |
| **Warnings** | تحذيرات | `"WARNINGS"` | List of warnings (invoice still accepted). |
| **Errors** | أخطاء | `"ERRORS"` | List of blocking errors (invoice rejected). |

---

## 3. EInvoiceMessage – Single Message Item

| C# Property | Arabic Label (API Docs) | JSON Key | Purpose / Notes |
|-------------|-------------------------|----------|-----------------|
| **Type** | نوع الرسالة | `"type"` | Enum `EInvoiceMessageType` (INFO, WARNING, ERROR). |
| **Status** | حالة الرسالة | `"status"` | Enum `EInvoiceMessageStatus` (PASS, WARNING, ERROR). |
| **Code** | كود الرسالة | `"EINV_CODE"` | JoFotara internal error or warning code. |
| **Category** | فئة الرسالة | `"EINV_CATEGORY"` | High-level category (e.g., Validation, Business Rules). |
| **Message** | نص الرسالة | `"EINV_MESSAGE"` | Human-readable Arabic or English message. |

---

## 4. Enum Values & Arabic Meanings

### 4.1 EInvoiceStatus – حالة الفاتورة
| Enum Value | Arabic Meaning | Description |
|------------|----------------|-------------|
| NOT_SUBMITTED | غير مرسلة | Invoice has not yet been sent. |
| SUBMITTED | مرسلة | Successfully received by JoFotara. |
| REJECTED | مرفوضة | Rejected due to validation errors. |
| ACCEPTED | مقبولة | Accepted and stored in JoFotara. |
| ALREADY_SUBMITTED | مرسلة مسبقًا | Duplicate submission detected. |

### 4.2 EInvoiceProcessingStatus – حالة المعالجة
| Enum Value | Arabic Meaning | Description |
|------------|----------------|-------------|
| PASS | نجحت | Processing completed without blocking errors. |
| ERROR | فشلت | Processing failed due to blocking errors. |

### 4.3 EInvoiceMessageType – نوع الرسالة
| Enum Value | Arabic Meaning | Description |
|------------|----------------|-------------|
| INFO | معلومة | Informational message (non-blocking). |
| WARNING | تحذير | Warning message (non-blocking). |
| ERROR | خطأ | Blocking error message. |

### 4.4 EInvoiceMessageStatus – حالة الرسالة
| Enum Value | Arabic Meaning | Description |
|------------|----------------|-------------|
| PASS | نجحت | Message indicates success. |
| WARNING | تحذير | Message indicates a warning. |
| ERROR | خطأ | Message indicates an error. |