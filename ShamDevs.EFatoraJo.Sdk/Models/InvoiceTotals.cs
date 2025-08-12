namespace ShamDevs.EFatoraJo.Models
{
    public class InvoiceTotals
    {
        public decimal TotalVATAmount { get; set; }
        public decimal TotalSpecialTaxAmount { get; set; }
        public decimal TotalBeforeDiscount { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal FinalPayableAmount { get; set; }
    }
}
