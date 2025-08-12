using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Utilities;

namespace ShamDevs.EFatoraJo.Tests.Builders
{
    public class InvoiceTotalsBuilder
    {
        private decimal _totalBeforeDiscount;
        private decimal _totalDiscountAmount;
        private decimal _totalVatAmount;
        private decimal _totalSpecialTaxAmount;
        private decimal _totalInvoiceAmount;
        private decimal _finalPayableAmount;
        private CurrencyCode _currency = CurrencyCode.JOD;

        public InvoiceTotalsBuilder WithTotalBeforeDiscount(decimal amount)
        {
            _totalBeforeDiscount = amount;
            return this;
        }

        public InvoiceTotalsBuilder WithTotalDiscountAmount(decimal amount)
        {
            _totalDiscountAmount = amount;
            return this;
        }

        public InvoiceTotalsBuilder WithTotalVatAmount(decimal amount)
        {
            _totalVatAmount = amount;
            return this;
        }

        public InvoiceTotalsBuilder WithTotalSpecialTaxAmount(decimal amount)
        {
            _totalSpecialTaxAmount = amount;
            return this;
        }

        public InvoiceTotalsBuilder WithTotalInvoiceAmount(decimal amount)
        {
            _totalInvoiceAmount = amount;
            return this;
        }

        public InvoiceTotalsBuilder WithFinalPayableAmount(decimal amount)
        {
            _finalPayableAmount = amount;
            return this;
        }

        public InvoiceTotalsBuilder WithCurrency(CurrencyCode currency)
        {
            _currency = currency;
            return this;
        }

        public InvoiceTotals Build()
        {
            return new InvoiceTotals
            {
                TotalBeforeDiscount = CurrencyHelper.Round(_totalBeforeDiscount, _currency),
                TotalDiscountAmount = CurrencyHelper.Round(_totalDiscountAmount, _currency),
                TotalVATAmount = CurrencyHelper.Round(_totalVatAmount, _currency),
                TotalSpecialTaxAmount = CurrencyHelper.Round(_totalSpecialTaxAmount, _currency),
                TotalInvoiceAmount = CurrencyHelper.Round(_totalInvoiceAmount, _currency),
                FinalPayableAmount = CurrencyHelper.Round(_finalPayableAmount, _currency)
            };
        }
    }
}