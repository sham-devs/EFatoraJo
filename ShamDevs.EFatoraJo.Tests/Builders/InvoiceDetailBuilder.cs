using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Utilities;

namespace ShamDevs.EFatoraJo.Tests.Builders
{
    public class InvoiceDetailBuilder
    {
        private string _id = "ITEM-001";
        private TaxCategoryCode _taxCategory = TaxCategoryCode.S;
        private string _description = "Test Product";
        private int _quantity = 1;
        private decimal _unitPriceBeforeTax = 100.00m;
        private decimal? _discountAmount = 0m;
        private decimal _specialTaxAmount = 0m;
        private decimal _totalBeforeTax;
        private decimal _taxAmount;
        private decimal _totalIncludingTax;
        private CurrencyCode _currency = CurrencyCode.JOD;

        #region Fluent API

        public InvoiceDetailBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public InvoiceDetailBuilder WithTaxCategory(TaxCategoryCode category)
        {
            _taxCategory = category;
            return this;
        }

        public InvoiceDetailBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public InvoiceDetailBuilder WithQuantity(int quantity)
        {
            _quantity = quantity;
            return this;
        }

        public InvoiceDetailBuilder WithUnitPrice(decimal price)
        {
            _unitPriceBeforeTax = price;
            return this;
        }

        public InvoiceDetailBuilder WithDiscountAmount(decimal? discount)
        {
            _discountAmount = discount;
            return this;
        }

        public InvoiceDetailBuilder WithSpecialTaxAmount(decimal amount)
        {
            _specialTaxAmount = amount;
            return this;
        }

        public InvoiceDetailBuilder WithTotalBeforeTax(decimal amount)
        {
            _totalBeforeTax = amount;
            return this;
        }

        public InvoiceDetailBuilder WithTaxAmount(decimal amount)
        {
            _taxAmount = amount;
            return this;
        }

        public InvoiceDetailBuilder WithTotalIncludingTax(decimal amount)
        {
            _totalIncludingTax = amount;
            return this;
        }

        public InvoiceDetailBuilder WithCurrency(CurrencyCode currency)
        {
            _currency = currency;
            return this;
        }

        #endregion

        public InvoiceDetail Build(InvoiceType invoiceType)
        {
            var detail = new InvoiceDetail(_id, _taxCategory, _description)
            {
                Quantity = _quantity,
                UnitPriceBeforeTax = _unitPriceBeforeTax,
                DiscountAmount = _discountAmount,
                SpecialTaxAmount = _specialTaxAmount
            };

            // Calculate with proper rounding when values weren't explicitly set
            if (_totalBeforeTax == 0)
            {
                var baseAmount = CurrencyHelper.Round(detail.UnitPriceBeforeTax * detail.Quantity, _currency);
                var discount = CurrencyHelper.Round(detail.DiscountAmount ?? 0m, _currency);
                detail.TotalBeforeTax = CurrencyHelper.Round(baseAmount - discount, _currency);
            }
            else
            {
                detail.TotalBeforeTax = _totalBeforeTax;
            }

            if (_taxAmount == 0)
            {
                // Tax includes both standard rate and special tax when applicable
                var baseTax = detail.TotalBeforeTax * detail.TaxRate;
                detail.TaxAmount = CurrencyHelper.Round(baseTax + _specialTaxAmount, _currency);
            }
            else
            {
                detail.TaxAmount = _taxAmount;
            }

            if (_totalIncludingTax == 0)
            {
                detail.TotalIncludingTax = CurrencyHelper.Round(
                    detail.TotalBeforeTax + detail.TaxAmount,
                    _currency);
            }
            else
            {
                detail.TotalIncludingTax = _totalIncludingTax;
            }

            return detail;
        }
    }
}