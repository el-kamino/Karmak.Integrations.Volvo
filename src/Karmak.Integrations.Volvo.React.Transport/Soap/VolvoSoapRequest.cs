using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public class VolvoSoapRequest {
        public IDictionary<string, IEnumerable<string>> Headers { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public SoapEnvelope Body { get; }
        public string Id => Body?.Header?.MessageID;

        public VolvoSoapRequest(SoapEnvelope body) {
            Body = body;
        }
    }
}
