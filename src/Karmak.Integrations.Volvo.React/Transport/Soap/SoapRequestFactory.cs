using Karmak.Integrations.Volvo.React.Utils;
using System;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public class SoapRequestFactory : ISoapRequestFactory
    {
        public SoapRequestFactory()
        {
        }

        public SoapEnvelope CreatePutRequest<T>(SoapMessageAddress address, T payload)
        {
            var putMessage = ToXml.Element(new PutMessage
            {
                Payload = new Payload
                {
                    Content = new Content
                    {
                        Value = ToXml.Element(payload)
                    }
                }
            });

            return CreateRequest(address, putMessage);
        }

        public SoapEnvelope CreateProcessRequest<T>(SoapMessageAddress address, T payload)
        {
            var processMessage = ToXml.Element(new ProcessMessage
            {
                Payload = new Payload
                {
                    Content = new Content
                    {
                        Value = ToXml.Element(payload)
                    }
                }
            });

            return CreateRequest(address, processMessage);
        }

        private SoapEnvelope CreateRequest<T>(SoapMessageAddress address, T payload)
        {
            var requestCreationTimestamp = DateTime.UtcNow;

            return new SoapEnvelope
            {
                Header = new Header
                {
                    To = address.To,
                    Action = address.Action,
                    MessageID = Guid.NewGuid().ToString(),
                    Security = new Security
                    {
                        Timestamp = new Timestamp
                        {
                            Id = Guid.NewGuid().ToString(),
                            Created = requestCreationTimestamp.InTimestampFormat(),
                            Expires = requestCreationTimestamp.AddMinutes(5).InTimestampFormat()
                        }
                    },
                    PayloadManifest = new PayloadManifest
                    {
                        Element = address.TargetService,
                        Version = address.TargetServiceVersion,
                        NamespaceUri = XmlNamespaces.StarUrl
                    },
                    VolvoDealerIdentity = new VolvoDealerIdentity
                    {
                        SiteCode = address.SiteCode
                    },
                    RespondTo = string.IsNullOrEmpty(address.RespondTo) ? null : new RespondTo
                    {
                        Endpoint = address.RespondTo
                    }
                },
                Body = new Body
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = ToXml.Element(payload)
                }
            };
        }
    }
}