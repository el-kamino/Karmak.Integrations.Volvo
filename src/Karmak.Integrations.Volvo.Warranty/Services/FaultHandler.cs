using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public sealed class FaultHandler : IShowServiceProcessingAdvisoryHandler
    {
        private readonly string _details;
        private readonly string _code;
        private readonly string _reason;

        public FaultHandler(string details = null, string code = null, string reason = null)
        {
            _details = details;
            _code = code;
            _reason = reason;
        }

        public Task<Result> HandleAsync(ShowServiceProcessingAdvisoryType request)
        {
            return Task.FromResult(Result.Fault(_code, _reason, _details));
        }
    }
}
