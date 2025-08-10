using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Utilities;

namespace ShamDevs.EFatoraJo.Tests.Builders
{
    public class InvoiceBuilder
    {
        private string _invoiceNumber = string.Concat("INV-", Guid.NewGuid().ToString().AsSpan(0, 8));
        private string _uniqueSerialNumber = Guid.NewGuid().ToString();
        private string _invoiceDate = DateTime.Now.ToString("yyyy-MM-dd");
        private InvoicePaymentTypeCode _invoicePaymentType = InvoicePaymentTypeCode.LocalGeneralSalesCash;
        private InvoiceType _invoiceType = InvoiceType.GeneralSales;
        private readonly string _invoiceNote = "Test Invoice";
        private CurrencyCode _currencyCode = CurrencyCode.JOD;

        private Supplier _supplier = new SupplierBuilder().Build();
        private Customer _customer = new CustomerBuilder().Build();
        private InvoiceTotals _invoiceTotals = new InvoiceTotalsBuilder().Build();
        private List<InvoiceDetail> _invoiceDetails = [];

        #region Fluent API

        public InvoiceBuilder WithInvoiceNumber(string number)
        {
            _invoiceNumber = number;
            return this;
        }

        public InvoiceBuilder WithUniqueSerialNumber(string serialNumber)
        {
            _uniqueSerialNumber = serialNumber;
            return this;
        }

        public InvoiceBuilder WithInvoiceDate(string date)
        {
            _invoiceDate = date;
            return this;
        }

        public InvoiceBuilder WithCurrency(CurrencyCode currency)
        {
            _currencyCode = currency;
            return this;
        }

        public InvoiceBuilder WithBasicDetails(string invoiceNumber, string invoiceDate)
        {
            _invoiceNumber = invoiceNumber;
            _invoiceDate = invoiceDate;
            return this;
        }

        public InvoiceBuilder WithBasicDetails(string invoiceNumber, string invoiceDate,
                                               InvoicePaymentTypeCode invoicePaymentType)
        {
            _invoiceNumber = invoiceNumber;
            _invoiceDate = invoiceDate;
            _invoicePaymentType = invoicePaymentType;
            return this;
        }

        public InvoiceBuilder WithInvoiceType(InvoiceType type)
        {
            _invoiceType = type;
            return this;
        }

        public InvoiceBuilder WithInvoicePaymentType(InvoicePaymentTypeCode type)
        {
            _invoicePaymentType = type;
            return this;
        }

        public InvoiceBuilder WithSupplier(Action<SupplierBuilder> configure)
        {
            var builder = new SupplierBuilder();
            configure(builder);
            _supplier = builder.Build();
            return this;
        }

        public InvoiceBuilder WithCustomer(Action<CustomerBuilder> configure)
        {
            var builder = new CustomerBuilder();
            configure(builder);
            _customer = builder.Build();
            return this;
        }

        public InvoiceBuilder WithTotals(Action<InvoiceTotalsBuilder> configure)
        {
            var builder = new InvoiceTotalsBuilder();
            configure(builder);
            _invoiceTotals = builder.Build();
            return this;
        }

        public InvoiceBuilder WithLineItem(Action<InvoiceDetailBuilder> configure)
        {
            var builder = new InvoiceDetailBuilder();
            configure(builder);
            _invoiceDetails.Add(builder.Build(_invoiceType));
            return this;
        }

        public InvoiceBuilder WithLineItems(params Action<InvoiceDetailBuilder>[] configurations)
        {
            foreach (var configure in configurations)
            {
                WithLineItem(configure);
            }
            return this;
        }

        public InvoiceBuilder WithMultipleLineItems(params Action<InvoiceDetailBuilder>[] configurations)
        {
            _invoiceDetails = [];
            foreach (var configure in configurations)
            {
                var builder = new InvoiceDetailBuilder();
                configure(builder);
                _invoiceDetails.Add(builder.Build(_invoiceType));
            }
            return this;
        }

        #endregion

        public Invoice Build()
        {
            if (_invoiceDetails.Count != 0)
            {
                // Recalculate totals only when they haven't been explicitly set (value == 0)
                if (_invoiceTotals.TotalBeforeDiscount == 0)
                {
                    _invoiceTotals.TotalBeforeDiscount = CurrencyHelper.Round(
                        _invoiceDetails.Sum(d => d.UnitPriceBeforeTax * d.Quantity),
                        _currencyCode);
                }

                if (_invoiceTotals.TotalDiscountAmount == 0)
                {
                    _invoiceTotals.TotalDiscountAmount = CurrencyHelper.Round(
                        _invoiceDetails.Sum(d => d.DiscountAmount ?? 0m),
                        _currencyCode);
                }

                if (_invoiceTotals.TotalVATAmount == 0)
                {
                    _invoiceTotals.TotalVATAmount = CurrencyHelper.Round(
                        _invoiceDetails.Sum(d => d.TaxAmount),
                        _currencyCode);
                }

                if (_invoiceTotals.TotalInvoiceAmount == 0)
                {
                    _invoiceTotals.TotalInvoiceAmount = CurrencyHelper.Round(
                        _invoiceDetails.Sum(d => d.TotalIncludingTax),
                        _currencyCode);
                }

                if (_invoiceTotals.FinalPayableAmount == 0 ||
                    _invoiceTotals.TotalBeforeDiscount != _invoiceDetails.Sum(d => d.UnitPriceBeforeTax * d.Quantity) ||
                    _invoiceTotals.TotalDiscountAmount != _invoiceDetails.Sum(d => d.DiscountAmount ?? 0m))
                {
                    _invoiceTotals.FinalPayableAmount = CurrencyHelper.Round(
                        _invoiceTotals.TotalBeforeDiscount - _invoiceTotals.TotalDiscountAmount,
                        _currencyCode);
                }
            }

            return new Invoice(
                _invoiceNumber,
                _uniqueSerialNumber,
                _invoiceDate,
                _invoicePaymentType,
                _supplier,
                _customer,
                _invoiceTotals,
                _invoiceDetails,
                _invoiceType)
            {
                InvoiceNote = _invoiceNote,
                Currency = _currencyCode
            };
        }
    }
}