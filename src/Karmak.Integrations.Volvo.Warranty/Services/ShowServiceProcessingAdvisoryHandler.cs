using System;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Extensions;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public sealed class ShowServiceProcessingAdvisoryHandler : IShowServiceProcessingAdvisoryHandler
    {
        private readonly IShowServiceProcessingAdvisoryHandler _claimReconHandler;
        private readonly IShowServiceProcessingAdvisoryHandler _claimStatusHandler;
        private readonly IShowServiceProcessingAdvisoryHandler _faultHandler;
        public ShowServiceProcessingAdvisoryHandler(IShowServiceProcessingAdvisoryHandler claimRecHandler, IShowServiceProcessingAdvisoryHandler claimStatusHandler, IShowServiceProcessingAdvisoryHandler faultHandler)
        {
            _claimReconHandler = claimRecHandler ?? throw new ArgumentNullException(nameof(claimRecHandler));
            _claimStatusHandler = claimStatusHandler ?? throw new ArgumentNullException(nameof(claimStatusHandler));
            _faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
        }

        public Task<Result> HandleAsync(ShowServiceProcessingAdvisoryType request)
        {
            var serviceId = request.ExtractServiceId();

            IShowServiceProcessingAdvisoryHandler delegatedHandler;
            switch (serviceId)
            {
                case Star.ClaimReconciliation:
                    delegatedHandler = _claimReconHandler;
                    break;
                case Star.ClaimStatus:
                    delegatedHandler = _claimStatusHandler;
                    break;
                default:
                    delegatedHandler = _faultHandler;
                    break;
            }

            return delegatedHandler.HandleAsync(request);
        }
    }
}
