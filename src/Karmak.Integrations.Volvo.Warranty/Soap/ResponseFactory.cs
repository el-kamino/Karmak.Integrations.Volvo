using System;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Utils;

namespace Karmak.Integrations.Volvo.Warranty.Soap
{
    public static class ResponseFactory
    {
        public static SoapEnvelope BuildEnvelope<T>(T body)
        {
            return new SoapEnvelope
            {
                Header = new Header
                {
                    Security = new Security
                    {
                        Timestamp = new Timestamp
                        {
                            Id = Guid.NewGuid().ToString(),
                            Created = DateTime.UtcNow.InTimestampFormat(),
                            Expires = DateTime.UtcNow
                                .AddMinutes(15)
                                .InTimestampFormat()
                        }
                    }
                },
                Body = new Body
                {
                    Value = ToXml.Element(body)
                }
            };
        }

        public static SoapEnvelope Fault(string detail = null, string code = null, string reason = null)
        {
            return BuildEnvelope(new Fault() { Detail = detail, FaultCode = code, FaultString = reason });
        }
    }
}
