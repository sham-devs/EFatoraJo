# Data Contracts - EFatoraJo

Field expectations and validation rules for invoices, return invoices, and responses.

---

## 1) Invoice

- Required core fields: `invoiceNumber`, `uniqueSerialNumber` (UUID preferred), `invoiceDate` (`yyyy-MM-dd`, not in the future), `type` (`Income`, `GeneralSales`, `SpecialSales`), `paymentType`, `currency` (`JOD` default), `supplier`, `customer`, `invoiceTotals`, `invoiceDetails`.
- Supplier: `taxVATNumber`, `incomeSourceSequence`, `registeredSupplierName`; optional contact/address fields align with existing enums/codes.
- Customer: `name` required; if `identificationType` is provided, `identificationNumber` must accompany it.
- Invoice details: list with at least one item. Required per item: `id`, `description`, `quantity`, `unitPriceBeforeTax`, `totalBeforeTax`, `taxAmount`, `totalIncludingTax`, `taxCategory`. Use `decimal` with the precision used in current models.
- Invoice totals: `totalBeforeDiscount`, `totalDiscountAmount`, `totalVATAmount`, `totalSpecialTaxAmount`, `totalInvoiceAmount`, `finalPayableAmount` must match the detail sums.
- Optional: `invoiceNote`, `postalCode`, `phoneNumber`, and similar descriptive fields.

### Normalization & Validation

- Dates must be `yyyy-MM-dd`; reject future dates.
- For sales invoices, quantities and amounts are positive; ensure currency consistency.
- Keep totals consistent with line items; fail fast on mismatches.

---

## 2) Sales Return Invoice

- Mirrors invoice fields but uses `returnInvoiceNumber` instead of `invoiceNumber`, adds `originalInvoiceNumber` and `returnReason`.
- Quantities and amounts should be negative to reflect returns; `type` and `paymentType` mirror the original invoice.

---

## 3) Request/Response Shape

- Requests send JSON with an `invoice` field containing Base64-encoded UBL. Do not log Base64 or invoice payloads.
- Responses (`EInvoiceResponse`) may include `success`, `qr`, `results/submissionId`, `einvStatus`, `einvNum`. Handle missing/nullable fields defensively.

---

## 4) JSON ↔ C# Mapping

- Public C# properties are PascalCase; JSON uses the configured serializer options (snake_case, case-insensitive). Do not change naming policy without strong reason.
- Use `decimal` for monetary fields and enums for controlled values (`InvoiceType`, `CurrencyCode`, `PaymentType`, `TaxCategoryCode`).
