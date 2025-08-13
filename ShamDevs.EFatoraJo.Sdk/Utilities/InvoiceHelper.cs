using System.Text;

namespace ShamDevs.EFatoraJo.Utilities
{
    public static class InvoiceHelper
    {
        // Helper method to serialize UBL to XML
        public static string SerializeUBL(UblSharp.InvoiceType ublInvoice)
        {
            var namespaces = new System.Xml.Serialization.XmlSerializerNamespaces();
            namespaces.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            namespaces.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            namespaces.Add("", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"); // Default namespace
            namespaces.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"); // Extension namespace

            using (var stringWriter = new Utf8StringWriter())
            {
                var xmlSettings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    OmitXmlDeclaration = false
                };

                using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, xmlSettings))
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(UblSharp.InvoiceType));
                    serializer.Serialize(xmlWriter, ublInvoice, namespaces);
                }

                return stringWriter.ToString();
            }
        }
    }
}