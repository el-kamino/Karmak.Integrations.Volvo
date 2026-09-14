using System.Xml;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public class FakeEncryptionHandler : IEncryptionHandler
    {
        public async Task<(bool success, XmlDocument response)> Execute(
                XmlDocument encryptedRequest,
                Func<XmlDocument, Task<(bool success, SoapEnvelope response)>> requestHandler)
        {
            var (success, response) = await requestHandler.Invoke(encryptedRequest);

            return (success, ToXml.Document(response));
        }
    }
}
