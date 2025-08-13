using System;

namespace ShamDevs.EFatoraJo.Exceptions
{
    public class EInvoiceException : Exception
    {
        public EInvoiceException(string message) : base(message) { }
        public EInvoiceException(string message, Exception inner) : base(message, inner) { }
    }
}
