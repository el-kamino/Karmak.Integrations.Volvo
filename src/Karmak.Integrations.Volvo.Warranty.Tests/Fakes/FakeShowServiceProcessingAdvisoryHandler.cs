using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public sealed class FakeShowServiceProcessingAdvisoryHandler : IShowServiceProcessingAdvisoryHandler
    {
        private readonly bool _success;
        public FakeShowServiceProcessingAdvisoryHandler(bool success = true) => _success = success;

        public Task<Result> HandleAsync(ShowServiceProcessingAdvisoryType request)
        {
            if (_success)
                return Task.FromResult(Result.Success());
            return Task.FromResult(Result.Fault("fail"));
        }
    }
}
