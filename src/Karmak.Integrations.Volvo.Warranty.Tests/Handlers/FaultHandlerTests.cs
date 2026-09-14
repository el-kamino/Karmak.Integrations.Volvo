using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Services;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Handlers
{
    public sealed class FaultHandlerTests
    {
        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_ReturnsAFault()
        {
            FaultHandler h = new FaultHandler("foo", "bar", "baz");

            Result r = await h.HandleAsync(new ShowServiceProcessingAdvisoryType());

            Assert.False(r.Succeeded);
            Assert.Equal("bar", r.FaultCode);
            Assert.Equal("foo", r.FaultDetails);
            Assert.Equal("baz", r.FaultReason);
        }
    }
}
