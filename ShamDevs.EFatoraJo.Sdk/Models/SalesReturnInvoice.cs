namespace ShamDevs.EFatoraJo.Models
{
    public class SalesReturnInvoice
    {
        public SalesReturnInvoice(
            string invoiceNumber,
            Invoice returnedInvoice,
            string uniqueSerialNumber,
            string invoiceDate,
            string returnReason)
        {
            InvoiceNumber = invoiceNumber;
            ReturnedInvoice = returnedInvoice;
            UniqueSerialNumber = uniqueSerialNumber;
            InvoiceDate = invoiceDate;
            ReturnReason = returnReason;
        }

        // Required properties
        public string InvoiceNumber { get; set; }
        public Invoice ReturnedInvoice { get; set; }
        public string UniqueSerialNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string ReturnReason { get; set; }
    }
}
