using Bogus;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Utils;

namespace Elk.Integrations.Volvo.Communications.Transport.Fakes {
    public static class FakeSoapEnvelope {
        public static SoapEnvelope GeneratePutMessage(Content content = null) => new Faker<SoapEnvelope>()
            .StrictMode(true)
            .RuleFor(r => r.Header, faker => new Header {
                MessageID = faker.Random.Guid().ToString(),
                To = faker.Random.AlphaNumeric(faker.Random.Int(5, 10)),
                Action = faker.Random.AlphaNumeric(faker.Random.Int(5, 10)),
                Security = new Security {
                    Timestamp = new Timestamp {
                        Id = faker.Random.Guid().ToString(),
                        Created = faker.Date.Recent().InTimestampFormat(),
                        Expires = faker.Date.Recent().InTimestampFormat()
                    }
                },
                PayloadManifest = new PayloadManifest {
                    Element = faker.Random.AlphaNumeric(faker.Random.Int(5, 10)),
                    Version = faker.Random.AlphaNumeric(faker.Random.Int(5, 10)),
                    NamespaceUri = faker.Random.AlphaNumeric(faker.Random.Int(5, 10))
                },
                VolvoDealerIdentity = new VolvoDealerIdentity {
                    SiteCode = faker.Random.Guid().ToString()
                }
            })
            .RuleFor(r => r.Body, faker => new Body {
                Id = faker.Random.Guid().ToString(),
                Value = ToXml.Element(new PutMessage {
                    Payload = new Payload {
                        Content = content ?? new Content()
                    }
                })
            })
            .Generate();
    }
}