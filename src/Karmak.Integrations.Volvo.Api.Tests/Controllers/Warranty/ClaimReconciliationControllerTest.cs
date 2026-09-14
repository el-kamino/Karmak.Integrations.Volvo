using Karmak.Integrations.Volvo.Api.Controllers.Warranty;
using Karmak.Integrations.Volvo.Api.Test.Helpers;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Xml;

namespace Karmak.Integrations.Volvo.Api.Test.Controllers.Warranty;

public class ClaimReconciliationControllerTest
{
    private readonly IVolvoWarrantyReconcilliationFetchService _fetchService;
    private readonly ClaimReconciliationController _controller;

    public ClaimReconciliationControllerTest()
    {
        _fetchService = Substitute.For<IVolvoWarrantyReconcilliationFetchService>();
        var sender = Substitute.For<IInboundMessageSender>();
        var logger = Substitute.For<ILogger<ClaimReconciliationController>>();

        _fetchService.TriggerReconciliationFetch(Arg.Any<ReconciliationFetchRequest>())
            .Returns((true, new SoapResult.Success(new XmlDocument())));

        _controller = new ClaimReconciliationController(_fetchService, sender, logger)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext()
        };
    }

    [Fact]
    public async Task WhenRequestIsSuccessfullyTriggered_It_ReturnsAccepted()
    {
        var request = new ReconciliationFetchRequest
        {
            StartDateTime = new DateTime(2020, 2, 1),
            EndDateTime = new DateTime(2020, 2, 2),
            PACode = "foobar"
        };

        var result = await _controller.Fetch(request);

        Assert.IsType<AcceptedResult>(result);
        await _fetchService.Received(1).TriggerReconciliationFetch(
            Arg.Is<ReconciliationFetchRequest>(m =>
                m.StartDateTime == request.StartDateTime
                && m.EndDateTime == request.EndDateTime));
    }

    [Fact]
    public async Task WhenStartAndEndDatesAreTheSame_It_ReturnsAccepted()
    {
        var request = new ReconciliationFetchRequest
        {
            StartDateTime = new DateTime(2020, 2, 2),
            EndDateTime = new DateTime(2020, 2, 2),
            PACode = "foobar"
        };

        var result = await _controller.Fetch(request);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WhenRequestIsMissingStartDate_It_ReturnsBadRequest()
    {
        var request = new ReconciliationFetchRequest
        {
            EndDateTime = new DateTime(2020, 2, 1)
        };

        var result = await _controller.Fetch(request);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task WhenRequestIsMissingEndDate_It_ReturnsBadRequest()
    {
        var request = new ReconciliationFetchRequest
        {
            StartDateTime = new DateTime(2020, 2, 1)
        };

        var result = await _controller.Fetch(request);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task WhenRequestStartDateIsAfterEndDate_It_ReturnsBadRequest()
    {
        var request = new ReconciliationFetchRequest
        {
            StartDateTime = new DateTime(2020, 2, 1),
            EndDateTime = new DateTime(2020, 1, 1)
        };

        var result = await _controller.Fetch(request);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task WhenRequestFailsToProcess_It_Returns500()
    {
        var request = new ReconciliationFetchRequest
        {
            StartDateTime = new DateTime(2020, 1, 1),
            EndDateTime = new DateTime(2020, 2, 1),
            PACode = "foobar"
        };

        _fetchService.TriggerReconciliationFetch(Arg.Any<ReconciliationFetchRequest>())
            .Returns((false, new SoapResult.Success(new XmlDocument())));

        var result = await _controller.Fetch(request);

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
