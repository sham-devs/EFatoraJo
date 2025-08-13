using ShamDevs.EFatoraJo.Enums;
using System.Collections.Generic;

namespace ShamDevs.EFatoraJo.Models
{
    public class Invoice
    {
        private CurrencyCode _currency = Enums.CurrencyCode.JOD;
        private InvoicePaymentTypeCode _invoiceType;

        public Invoice(
    string invoiceNumber,
    string uniqueSerialNumber,
    string invoiceDate,
    InvoicePaymentTypeCode paymentType,
    Supplier supplier,
    Customer customer,
    InvoiceTotals invoiceTotals,
    List<InvoiceDetail> invoiceDetails,
    InvoiceType type = InvoiceType.GeneralSales)   // <-- NEW, optional
        {
            InvoiceNumber = invoiceNumber;
            UniqueSerialNumber = uniqueSerialNumber;
            InvoiceDate = invoiceDate;
            PaymentType = paymentType;
            Supplier = supplier;
            Customer = customer;
            InvoiceTotals = invoiceTotals;
            InvoiceDetails = invoiceDetails;
            Type = type;
        }


        public InvoiceType Type { get; private set; } = InvoiceType.GeneralSales;

        // Required properties (set via constructor)
        public string InvoiceNumber { get; set; }
        public string UniqueSerialNumber { get; set; }
        public string InvoiceDate { get; set; }
        public InvoicePaymentTypeCode PaymentType { get => _invoiceType; set => _invoiceType = value; }
        public Supplier Supplier { get; set; }
        public Customer Customer { get; set; }
        public InvoiceTotals InvoiceTotals { get; set; }
        public List<InvoiceDetail> InvoiceDetails { get; set; }

        // Optional properties
        public string? InvoiceNote { get; set; }

        // Currency property with backing field
        public CurrencyCode Currency
        {
            get => _currency;
            set => _currency = value;
        }

        // Read-only string representations for serialization
        public string CurrencyCode => Currency.GetStringValue();
        public string InvoiceTypeCode => PaymentType.GetStringValue();
    }
}