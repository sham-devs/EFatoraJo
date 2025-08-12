using ShamDevs.EFatoraJo.Enums;

namespace ShamDevs.EFatoraJo.Models
{
    public class InvoiceDetail
    {
        public string ID { get; set; }
        public TaxCategoryCode TaxCategory { get; private set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceBeforeTax { get; set; }
        public decimal TotalBeforeTax { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalIncludingTax { get; set; }

        private decimal? _taxRate;

        // Read-only property for TaxPercent as it should always come from TaxCategory
        public decimal TaxPercent => TaxCategory.GetTaxPercent();
        public decimal? SpecialTaxAmount { get; set; }
        public decimal TotalSpecialTax => SpecialTaxAmount ?? 0m;

        // TaxRate can be set or calculated from TaxCategory
        public decimal TaxRate
        {
            get => _taxRate ?? (TaxPercent / 100m);
            set => _taxRate = value;
        }

        public InvoiceDetail(string id, TaxCategoryCode taxCategory, string description)
        {
            ID = id;
            TaxCategory = taxCategory;
            Description = description;
        }

        public void UpdateTaxCategory(TaxCategoryCode newCategory)
        {
            TaxCategory = newCategory;
            // Reset the custom TaxRate when category changes
            _taxRate = null;
        }
    }
}