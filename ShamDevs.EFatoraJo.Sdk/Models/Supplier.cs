namespace ShamDevs.EFatoraJo.Models
{
    public class Supplier
    {
        public Supplier(
            string taxVATNumber,
            string incomeSourceSequence,
            string registeredSupplierName)
        {
            TaxVATNumber = taxVATNumber;
            IncomeSourceSequence = incomeSourceSequence;
            RegisteredSupplierName = registeredSupplierName;
        }

        // Required properties
        public string TaxVATNumber { get; set; }
        public string IncomeSourceSequence { get; set; }
        public string RegisteredSupplierName { get; set; }
    }
}
