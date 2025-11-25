# Single Invoice Processor - دليل الاستخدام

## نظرة عامة

سكريبت Python لمعالجة فاتورة واحدة من ملف PDF وإرسالها إلى نظام الفوترة الإلكترونية الأردني (Jofotara).

## الوظائف

1. **قراءة ملف PDF**: استخراج النص من ملف PDF واحد محدد
2. **استخراج البيانات**: تحليل بيانات الفاتورة (رقم الفاتورة، التاريخ، العميل، البنود، المجاميع)
3. **توليد JSON للفاتورة الأصلية**: بناء ملف JSON مطابق لمتطلبات النظام
4. **توليد JSON لفاتورة الإرجاع**: إنشاء فاتورة إرجاع (دائنة) مرتبطة بالفاتورة الأصلية
5. **حفظ الملفات**: تخزين ملفات JSON في نفس المجلد الخاص بالـ PDF
6. **الإرسال**: إرسال الفاتورتين إلى نظام الفوترة والتحقق من نجاح العملية
7. **التقارير**: إظهار رسائل واضحة في حالة النجاح أو الفشل

---

## المتطلبات

### 1. Python 3.8+

تأكد من تثبيت Python:
```bash
python --version
```

### 2. تثبيت المكتبات المطلوبة

```bash
pip install -r requirements.txt
```

أو تثبيت PyPDF2 مباشرة:
```bash
pip install PyPDF2
```

### 3. بناء EFatoraJoConsoleApp

```bash
cd EFatoraJoConsoleApp
dotnet build -c Release
```

### 4. إعداد User Secrets

```bash
dotnet user-secrets set "ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "SecretKey" "YOUR_SECRET_KEY"
dotnet user-secrets set "Supplier:TaxVATNumber" "50185012"
dotnet user-secrets set "Supplier:IncomeSourceSequence" "3905659"
dotnet user-secrets set "Supplier:RegisteredSupplierName" "باسل حسين مسلم جمعه"
```

---

## الاستخدام

### الصيغة الأساسية

```bash
python process_single_invoice.py --pdf-path <path_to_pdf> --original-uuid <uuid>
```

### المعاملات (Parameters)

| المعامل | وصف | مطلوب | مثال |
|---------|------|-------|------|
| `--pdf-path` | مسار ملف PDF للفاتورة | نعم ✓ | `temp/temp/INV-001.pdf` |
| `--original-uuid` | UUID للفاتورة الأصلية | نعم ✓ | `550e8400-e29b-41d4-a716-446655440000` |
| `--console-app-path` | مسار EFatoraJoConsoleApp.dll | لا | `EFatoraJoConsoleApp/bin/Release/net8.0/EFatoraJoConsoleApp.dll` |
| `--return-reason` | سبب الإرجاع | لا | `Return for testing purposes` |

---

## أمثلة عملية

### مثال 1: استخدام أساسي

```bash
python process_single_invoice.py \
  --pdf-path "temp/temp/INV-2024-001.pdf" \
  --original-uuid "550e8400-e29b-41d4-a716-446655440000"
```

### مثال 2: مع تحديد سبب الإرجاع

```bash
python process_single_invoice.py \
  --pdf-path "temp/temp/INV-2024-001.pdf" \
  --original-uuid "550e8400-e29b-41d4-a716-446655440000" \
  --return-reason "إرجاع كامل - طلب العميل"
```

### مثال 3: مع مسار مخصص لـ Console App

```bash
python process_single_invoice.py \
  --pdf-path "C:\Invoices\INV-001.pdf" \
  --original-uuid "550e8400-e29b-41d4-a716-446655440000" \
  --console-app-path "C:\EFatoraJo\EFatoraJoConsoleApp.dll"
```

### مثال 4: استخدام على Windows

```powershell
python process_single_invoice.py `
  --pdf-path "C:\Users\basel\Documents\Invoices\INV-2024-001.pdf" `
  --original-uuid "550e8400-e29b-41d4-a716-446655440000"
```

---

## سير العمل (Workflow)

### الخطوة 1: قراءة PDF
```
[INFO] Reading PDF: INV-2024-001.pdf
[INFO] PDF has 1 page(s)
[INFO] Extracted text from page 1 (2450 characters)
[INFO] Total extracted text: 2450 characters
```

### الخطوة 2: استخراج البيانات
```
[INFO] Parsing invoice data from PDF text...
[INFO]   Found invoice number: INV-2024-001
[INFO]   Found invoice date: 2024-11-24
[INFO]   Found customer name: Ahmad Hassan
[INFO]   Found customer ID: 9876543210
[INFO]   Found line item 1: Consulting Services
[INFO]   Found total before_discount: 1000.0
```

### الخطوة 3: توليد JSON
```
[INFO] Building original invoice JSON structure...
[INFO] Original invoice JSON created: INV-2024-001
[INFO] Building return invoice JSON structure...
[INFO] Return invoice JSON created: RET-INV-2024-001
```

### الخطوة 4: حفظ الملفات
```
[INFO] Saved JSON file: INV-2024-001_original.json
[INFO] Saved JSON file: INV-2024-001_return.json
Files saved successfully:
  Original Invoice: temp/temp/INV-2024-001_original.json
  Return Invoice: temp/temp/INV-2024-001_return.json
```

### الخطوة 5: الإرسال
```
[INFO] Submitting invoices to e-invoicing system...
[INFO] Executing command: dotnet EFatoraJoConsoleApp.dll --invoice-file ...
[SUCCESS] ✓ SUCCESS: Invoices submitted successfully!
  Original Invoice: INV-2024-001
  Return Invoice: RET-INV-2024-001
  QR Code: iVBORw0KGgoAAAANSUhEUgAA...
```

---

## شرح التدفق الكامل

### 1. قراءة PDF (PDF Reading)

يستخدم السكريبت مكتبة PyPDF2 لقراءة النص من ملف PDF:

```python
import PyPDF2
with open(pdf_path, 'rb') as f:
    reader = PyPDF2.PdfReader(f)
    text = ""
    for page in reader.pages:
        text += page.extract_text()
```

**ملاحظة**: يجب أن يكون PDF يحتوي على نص قابل للاستخراج (ليس صورة ممسوحة).

### 2. استخراج البيانات (Data Extraction)

يستخدم السكريبت Regular Expressions لاستخراج:

#### بيانات الفاتورة الأساسية:
- رقم الفاتورة: `رقم الفاتورة: INV-XXX` أو `Invoice Number: XXX`
- التاريخ: `YYYY-MM-DD` أو `DD/MM/YYYY`

#### بيانات العميل:
- الاسم: `اسم العميل:` أو `Customer Name:`
- الرقم الوطني: `الرقم الوطني:` أو `National ID:`
- الهاتف: `رقم الهاتف:` أو `Phone:`
- الرمز البريدي: `الرمز البريدي:` أو `Postal Code:`

#### بنود الفاتورة:
- الوصف، الكمية، السعر، الخصم، المجموع

#### المجاميع:
- المجموع قبل الخصم
- قيمة الخصم الكلي
- المبلغ النهائي

### 3. بناء JSON للفاتورة الأصلية

```json
{
  "invoiceNumber": "INV-2024-001",
  "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
  "invoiceDate": "2024-11-24",
  "paymentType": "LocalIncomeCash",
  "type": "Income",
  "currency": "JOD",
  "supplier": {
    "taxVATNumber": "50185012",
    "incomeSourceSequence": "3905659",
    "registeredSupplierName": "باسل حسين مسلم جمعه"
  },
  "customer": {
    "name": "Ahmad Hassan",
    "identificationNumber": "9876543210",
    "identificationType": "NIN",
    "phoneNumber": "962791234567",
    "postalCode": "11110"
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
      "id": "1",
      "taxCategory": "O",
      "description": "Consulting Services",
      "quantity": 10,
      "unitPriceBeforeTax": 100.0,
      "totalBeforeTax": 1000.0,
      "taxAmount": 0.0,
      "totalIncludingTax": 1000.0
    }
  ],
  "invoiceNote": "Professional services"
}
```

### 4. بناء JSON لفاتورة الإرجاع

فاتورة الإرجاع تُبنى بنفس هيكل الفاتورة الأصلية مع الفروقات التالية:

| الحقل | الفاتورة الأصلية | فاتورة الإرجاع |
|-------|------------------|-----------------|
| **invoiceNumber** | `INV-2024-001` | `RET-INV-2024-001` |
| **uniqueSerialNumber** | من CLI parameter | UUID جديد مولد تلقائيًا |
| **invoiceDate** | من PDF | تاريخ اليوم |
| **invoiceNote** | من PDF | سبب الإرجاع |
| **البنود والمبالغ** | قيم موجبة | نفس القيم (SDK يتعامل معها) |

**ملاحظة مهمة**:
- القيم في JSON تبقى **موجبة** (positive)
- SDK الخاص بـ EFatoraJo يتولى تحويلها إلى سالبة عند توليد XML
- حسب الكود في `SalesReturnInvoice.cs` و `InvoiceCommandHandler.cs`

### 5. الإرسال إلى النظام

يستخدم السكريبت EFatoraJoConsoleApp لإرسال الفاتورتين معًا:

```bash
dotnet EFatoraJoConsoleApp.dll \
  --invoice-file INV-2024-001_original.json \
  --return-file INV-2024-001_return.json \
  --output-format json
```

حسب PRODUCTION-GUIDE.md (السطور 311-337):
- إذا كانت الفاتورة الأصلية مُرسلة مسبقًا، سيتم تخطيها
- سيتم إرسال فاتورة الإرجاع فقط
- النظام يربط فاتورة الإرجاع بالفاتورة الأصلية عبر `BillingReference` في XML

---

## الفرق بين الفاتورة العادية وفاتورة الإرجاع في XML

### الفاتورة العادية (Invoice)

```xml
<Invoice xmlns="urn:oasis:names:specification:ubl:schema:xsd:Invoice-2">
  <cbc:InvoiceTypeCode>388</cbc:InvoiceTypeCode>
  <cbc:ID>INV-2024-001</cbc:ID>
  <cbc:UUID>550e8400-e29b-41d4-a716-446655440000</cbc:UUID>
  <cbc:IssueDate>2024-11-24</cbc:IssueDate>

  <!-- لا يوجد BillingReference -->

  <cac:InvoiceLine>
    <cbc:InvoicedQuantity>10</cbc:InvoicedQuantity>
    <cbc:LineExtensionAmount>1000.0</cbc:LineExtensionAmount>
  </cac:InvoiceLine>
</Invoice>
```

### فاتورة الإرجاع (Return/Credit Note)

```xml
<Invoice xmlns="urn:oasis:names:specification:ubl:schema:xsd:Invoice-2">
  <cbc:InvoiceTypeCode>381</cbc:InvoiceTypeCode>
  <cbc:ID>RET-INV-2024-001</cbc:ID>
  <cbc:UUID>NEW-UUID-FOR-RETURN</cbc:UUID>
  <cbc:IssueDate>2024-11-24</cbc:IssueDate>

  <!-- ربط بالفاتورة الأصلية -->
  <cac:BillingReference>
    <cac:InvoiceDocumentReference>
      <cbc:ID>INV-2024-001</cbc:ID>
      <cbc:UUID>550e8400-e29b-41d4-a716-446655440000</cbc:UUID>
      <cbc:IssueDate>2024-11-24</cbc:IssueDate>
    </cac:InvoiceDocumentReference>
  </cac:BillingReference>

  <!-- الكميات والمبالغ سالبة -->
  <cac:InvoiceLine>
    <cbc:InvoicedQuantity>-10</cbc:InvoicedQuantity>
    <cbc:LineExtensionAmount>-1000.0</cbc:LineExtensionAmount>
  </cac:InvoiceLine>
</Invoice>
```

**الفروقات الرئيسية**:
1. **InvoiceTypeCode**: `388` للفاتورة العادية، `381` لفاتورة الإرجاع
2. **BillingReference**: موجودة فقط في فاتورة الإرجاع وتشير للفاتورة الأصلية
3. **الكميات والمبالغ**: موجبة في الفاتورة العادية، سالبة في فاتورة الإرجاع

---

## معالجة الأخطاء

### Exit Codes

| Code | الوصف | السبب المحتمل |
|------|-------|---------------|
| `0` | نجاح | تم إرسال الفواتير بنجاح |
| `1` | فشل الإرسال | خطأ من API أو validation |
| `2` | خطأ في قراءة PDF | ملف غير موجود أو تالف |
| `3` | خطأ في استخراج البيانات | بنية PDF غير متوافقة |
| `4` | خطأ في الإرسال | Console App غير موجود أو credentials خاطئة |
| `99` | خطأ غير متوقع | خطأ برمجي غير معروف |

### أمثلة على الأخطاء

#### خطأ: ملف PDF غير موجود

```
[ERROR] PDF Read Error: PDF file not found: temp/temp/INV-001.pdf

✗ ERROR: PDF file not found: temp/temp/INV-001.pdf

Please verify:
  - The PDF file exists and is readable
  - The PDF contains extractable text (not scanned images)
```

**الحل**: تأكد من صحة مسار الملف.

---

#### خطأ: UUID غير صحيح

```
ERROR: Invalid UUID format: abc123

Please provide a valid UUID in the format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

**الحل**: استخدم UUID صحيح، مثل: `550e8400-e29b-41d4-a716-446655440000`

---

#### خطأ: بيانات ناقصة من PDF

```
[ERROR] Data Extraction Error: Could not extract invoice number from PDF

✗ ERROR: Could not extract invoice number from PDF

Please verify:
  - The PDF structure matches the expected format
  - The PDF contains valid invoice data in Arabic or English
```

**الحل**: تأكد من أن PDF يحتوي على البيانات المطلوبة بالصيغة المتوقعة.

---

#### خطأ: فشل الإرسال

```
✗ SUBMISSION FAILED
  Error Type: ValidationError
  Message: Invoice validation failed
  Errors:
    - InvoiceDate: Invoice date cannot be in the future
    - Customer.TaxNumber: Required for SpecialSales invoices
```

**الحل**: راجع الأخطاء وصحح البيانات في PDF.

---

#### خطأ: Console App غير موجود

```
✗ ERROR: EFatoraJoConsoleApp not found at: EFatoraJoConsoleApp/bin/Release/net8.0/EFatoraJoConsoleApp.dll
Please build the application first using: dotnet build -c Release
```

**الحل**: قم ببناء التطبيق:
```bash
cd EFatoraJoConsoleApp
dotnet build -c Release
```

---

## نصائح مهمة

### 1. صيغة PDF
- يجب أن يحتوي PDF على نص قابل للاستخراج (ليس صورة ممسوحة)
- البيانات يجب أن تكون بصيغة واضحة مع Labels مناسبة
- اللغة العربية والإنجليزية مدعومة

### 2. UUID الفاتورة الأصلية
- احفظ UUID لكل فاتورة أصلية
- ستحتاجه لإنشاء فواتير الإرجاع
- يجب أن يكون فريد لكل فاتورة

### 3. التحقق من الإرسال
- السكريبت يتحقق تلقائيًا من نجاح الإرسال
- في حالة الفشل، يعرض رسالة خطأ واضحة
- راجع الأخطاء وصححها قبل إعادة المحاولة

### 4. الملفات المُولدة
- يتم حفظ JSON في نفس مجلد PDF
- احتفظ بنسخة احتياطية من الملفات
- يمكنك مراجعة JSON قبل الإرسال

---

## استكشاف الأخطاء (Troubleshooting)

### مشكلة: "PyPDF2 not installed"

```bash
pip install PyPDF2
```

### مشكلة: "dotnet command not found"

تأكد من تثبيت .NET 8.0 SDK:
```bash
dotnet --version
```

### مشكلة: "User secrets not configured"

```bash
cd EFatoraJoConsoleApp
dotnet user-secrets list
```

إذا كانت فارغة، قم بإعدادها:
```bash
dotnet user-secrets set "ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "SecretKey" "YOUR_SECRET_KEY"
```

### مشكلة: PDF لا يحتوي على نص قابل للاستخراج

إذا كان PDF عبارة عن صورة ممسوحة، استخدم OCR:
```bash
# استخدم أداة OCR لتحويل الصورة إلى نص أولاً
# مثل: Adobe Acrobat, Tesseract OCR, أو أي أداة أخرى
```

---

## الملفات الناتجة

بعد التشغيل الناجح، ستجد في مجلد PDF:

```
temp/temp/
├── INV-2024-001.pdf                    # الملف الأصلي
├── INV-2024-001_original.json          # JSON للفاتورة الأصلية
└── INV-2024-001_return.json            # JSON لفاتورة الإرجاع
```

### محتوى ملفات JSON

#### INV-2024-001_original.json
```json
{
  "invoiceNumber": "INV-2024-001",
  "uniqueSerialNumber": "550e8400-e29b-41d4-a716-446655440000",
  "invoiceDate": "2024-11-24",
  "paymentType": "LocalIncomeCash",
  "type": "Income",
  "currency": "JOD",
  "supplier": { ... },
  "customer": { ... },
  "invoiceTotals": { ... },
  "invoiceDetails": [ ... ]
}
```

#### INV-2024-001_return.json
```json
{
  "invoiceNumber": "RET-INV-2024-001",
  "uniqueSerialNumber": "NEW-GENERATED-UUID",
  "invoiceDate": "2024-11-24",
  "paymentType": "LocalIncomeCash",
  "type": "Income",
  "currency": "JOD",
  "supplier": { ... },
  "customer": { ... },
  "invoiceTotals": { ... },
  "invoiceDetails": [ ... ],
  "invoiceNote": "Return for testing purposes"
}
```

---

## الدعم والمساعدة

### الحصول على مساعدة

```bash
python process_single_invoice.py --help
```

### الإبلاغ عن مشاكل

إذا واجهت أي مشكلة:
1. راجع رسائل الخطأ بعناية
2. تحقق من المتطلبات (Python, .NET, PyPDF2)
3. تأكد من إعداد User Secrets
4. راجع ملف PRODUCTION-GUIDE.md

---

## الترخيص

هذا السكريبت جزء من مشروع EFatoraJo - Jordan E-Invoicing System.

---

**آخر تحديث**: 2024-11-24
**الإصدار**: 1.0.0
