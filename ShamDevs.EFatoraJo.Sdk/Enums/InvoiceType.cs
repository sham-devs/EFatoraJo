namespace ShamDevs.EFatoraJo.Enums
{
    public enum InvoiceType
    {
        Income,        // no tax rows at all
        GeneralSales,  // only the existing VAT row
        SpecialSales   // VAT + OTH
    }
}
