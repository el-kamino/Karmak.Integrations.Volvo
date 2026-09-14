using System.Text;
using System.Xml;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils
{
    public static class ResourceRetriever
    {

        public static XmlDocument GetDocument(string fileName)
        {
            return GetResource(fileName, bytes =>
            {
                var xmlDocument = new XmlDocument { PreserveWhitespace = true };
                xmlDocument.LoadXml(Encoding.UTF8.GetString(bytes));
                return xmlDocument;
            });
        }

        public static T GetResource<T>(string filename, Func<byte[], T> action)
        {
            using (var resourceStream = typeof(ResourceRetriever).Assembly.GetManifestResourceStream(ResourceName(filename)))
            {
                if (resourceStream == null)
                {
                    throw new ArgumentException($"Unable to find resource: '{ResourceName(filename)}'", nameof(filename));
                }

                using (var memoryStream = new MemoryStream())
                {
                    resourceStream.CopyTo(memoryStream);
                    return action.Invoke(memoryStream.ToArray());
                }
            }
        }

        private static string ResourceName(string filename)
        {
            return $"{typeof(ResourceRetriever).Namespace?.Replace(".TestUtils", "")}.Resources.{filename}";
        }
    }
}
