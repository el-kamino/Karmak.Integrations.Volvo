using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class XDocumentExtensions {
        public static byte[] ToByteArray(this XDocument document, Encoding encoding) {
            using (var stream = new MemoryStream()) {
                using (var writer = new StreamWriter(stream, encoding)) {
                    document.Save(writer);
                    return stream.ToArray();
                }
            }
        }
    }
}