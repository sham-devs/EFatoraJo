using ShamDevs.EFatoraJo.Models;

namespace ShamDevs.EFatoraJo.Tests.Builders
{
    public class ReturnInvoiceBuilder
    {
        private string _invoiceNumber = string.Concat("RINV-", Guid.NewGuid().ToString().AsSpan(0, 8));
        private string _uniqueSerialNumber = Guid.NewGuid().ToString();
        private string _invoiceDate = DateTime.Now.ToString("yyyy-MM-dd");
        private string _returnReason = "Defective product";
        private Invoice _returnedInvoice = new InvoiceBuilder().Build();

        public ReturnInvoiceBuilder WithInvoiceNumber(string number)
        {
            _invoiceNumber = number;
            return this;
        }

        public ReturnInvoiceBuilder WithUniqueSerialNumber(string serialNumber)
        {
            _uniqueSerialNumber = serialNumber;
            return this;
        }

        public ReturnInvoiceBuilder WithInvoiceDate(string date)
        {
            _invoiceDate = date;
            return this;
        }

        public ReturnInvoiceBuilder WithReturnReason(string reason)
        {
            _returnReason = reason;
            return this;
        }

        public ReturnInvoiceBuilder WithOriginalInvoice(Action<InvoiceBuilder> configure)
        {
            var builder = new InvoiceBuilder();
            configure(builder);
            _returnedInvoice = builder.Build();
            return this;
        }

        public SalesReturnInvoice Build()
        {
            return new SalesReturnInvoice(
                _invoiceNumber,
                _returnedInvoice,
                _uniqueSerialNumber,
                _invoiceDate,
                _returnReason);
        }
    }
}
