using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Elk.Integrations.Volvo.Communications.Transport.Tests.Utils {
    public static class ResourceRetriever {
        public static X509Certificate2 GetCert(string filename, string password) {
            return GetResource(filename, bytes => new X509Certificate2(bytes, password));
        }

        private static T GetResource<T>(string filename, Func<byte[], T> action) {
            using (var resourceStream = typeof(ResourceRetriever).Assembly.GetManifestResourceStream(ResourceName(filename))) {
                if (resourceStream == null) {
                    throw new ArgumentException($"Unable to find resource: '{ResourceName(filename)}'", nameof(filename));
                }

                using (var memoryStream = new MemoryStream()) {
                    resourceStream.CopyTo(memoryStream);
                    return action.Invoke(memoryStream.ToArray());
                }
            }
        }

        private static string ResourceName(string filename) {
            return $"{typeof(ResourceRetriever).Namespace?.Replace(".Utils", "")}.Resources.{filename}";
        }
    }
}
