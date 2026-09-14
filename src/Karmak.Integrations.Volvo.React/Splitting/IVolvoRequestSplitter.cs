using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Transport.Soap;

namespace Karmak.Integrations.Volvo.React.Splitting
{
    public interface IVolvoRequestSplitter {
        IEnumerable<SoapEnvelope> Split(SoapEnvelope envelope);
    }
}