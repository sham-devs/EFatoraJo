using System.IO;
using System.Text;

namespace ShamDevs.EFatoraJo.Utilities
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
