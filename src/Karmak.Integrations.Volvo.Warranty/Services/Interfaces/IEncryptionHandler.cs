using System;
using System.Threading.Tasks;
using System.Xml;
using Karmak.Integrations.Volvo.React.Transport.Soap;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IEncryptionHandler
    {
        Task<(bool success, XmlDocument response)> Execute(
            XmlDocument encryptedRequest,
            Func<XmlDocument, Task<(bool success, SoapEnvelope response)>> requestHandler);
    }
}
