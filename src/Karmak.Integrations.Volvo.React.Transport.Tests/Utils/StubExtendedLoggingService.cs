using System.Collections.Generic;
using System.Threading.Tasks;
using Elk.Core.ExtendedLogging;

namespace Elk.Integrations.Volvo.Communications.Transport.Tests.Utils {
    public class StubExtendedLoggingService : IExtendedLoggingService {
        public IList<ExtendedLoggingRequest> LogEvents { get; } = new List<ExtendedLoggingRequest>();

        public Task<ExtendedLogReference> Execute(ExtendedLoggingRequest request) {
            LogEvents.Add(request);

            return Task.FromResult(new ExtendedLogReference());
        }
    }
}