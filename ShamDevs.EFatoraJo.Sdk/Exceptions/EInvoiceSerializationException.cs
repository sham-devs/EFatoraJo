using System;

namespace ShamDevs.EFatoraJo.Exceptions
{
    public class EInvoiceSerializationException : EInvoiceException
    {
        public EInvoiceSerializationException(string message) : base(message) { }
        public EInvoiceSerializationException(string message, Exception inner) : base(message, inner) { }
    }
}
