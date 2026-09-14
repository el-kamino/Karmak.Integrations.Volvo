using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Handlers
{
    public sealed class ShowServiceProcessingAdvisoryHandlerTests
    {
        private readonly IShowServiceProcessingAdvisoryHandler _reconciliationHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
        private readonly IShowServiceProcessingAdvisoryHandler _statusHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
        private readonly IShowServiceProcessingAdvisoryHandler _faultHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();

        [Fact]
        public void WhenCtorParametersAreNull_It_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { new ShowServiceProcessingAdvisoryHandler(null, _statusHandler, _faultHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ShowServiceProcessingAdvisoryHandler(_reconciliationHandler, null, _faultHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ShowServiceProcessingAdvisoryHandler(_reconciliationHandler, _statusHandler, null); });
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalledAndServiceIdIsUnrecognized_It_ReturnsAFault()
        {
            var fault = new FaultHandler(code: "500");

            ShowServiceProcessingAdvisoryHandler h = new ShowServiceProcessingAdvisoryHandler(_reconciliationHandler, _statusHandler, fault);

            var result = await h.HandleAsync(new ShowServiceProcessingAdvisoryType()
            {
                ApplicationArea = new ApplicationAreaType()
                {
                    Sender = new SenderType()
                    {
                        ServiceID = new IdentifierType()
                        {
                            Value = "Unrecognized"
                        }
                    }
                }
            });

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalledAndServiceIdIsRecognized_It_CallsTheRightHandler()
        {
            var reconRequest = new ShowServiceProcessingAdvisoryType()
            {
                ApplicationArea = new ApplicationAreaType()
                {
                    Sender = new SenderType()
                    {
                        ServiceID = new IdentifierType()
                        {
                            Value = Star.ClaimReconciliation
                        }
                    }
                }
            };

            var statusRequest = new ShowServiceProcessingAdvisoryType()
            {
                ApplicationArea = new ApplicationAreaType()
                {
                    Sender = new SenderType()
                    {
                        ServiceID = new IdentifierType()
                        {
                            Value = Star.ClaimStatus
                        }
                    }
                }
            };

            var recon = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            recon.HandleAsync(Arg.Is<ShowServiceProcessingAdvisoryType>(r => ReferenceEquals(r, reconRequest))).Returns(Task.FromResult(Result.Success()));
            var status = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            status.HandleAsync(Arg.Is<ShowServiceProcessingAdvisoryType>(r => ReferenceEquals(r, statusRequest))).Returns(Task.FromResult(Result.Success()));
            var fault = Substitute.For<IShowServiceProcessingAdvisoryHandler>();

            ShowServiceProcessingAdvisoryHandler h = new ShowServiceProcessingAdvisoryHandler(recon, status, fault);

            var result = await h.HandleAsync(reconRequest);

            Assert.True(result.Succeeded);
            await recon.Received(1).HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>());

            result = await h.HandleAsync(statusRequest);
            Assert.True(result.Succeeded);
            await status.Received(1).HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>());
        }
    }
}
