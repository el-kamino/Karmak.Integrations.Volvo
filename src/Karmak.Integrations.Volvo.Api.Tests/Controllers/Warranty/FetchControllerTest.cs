using System.Xml;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Api.Controllers.Warranty;
using Karmak.Integrations.Volvo.Api.Test.Helpers;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Karmak.Integrations.Volvo.Api.Test.Controllers.Warranty;

public class FetchControllerTest
{
    private readonly IWarrantyElkContextRetriever _authenticationClient;
    private readonly IVolvoWarrantyReconcilliationFetchService _fetchService;
    private readonly FetchController _controller;

    public FetchControllerTest()
    {
        _authenticationClient = Substitute.For<IWarrantyElkContextRetriever>();
        _fetchService = Substitute.For<IVolvoWarrantyReconcilliationFetchService>();
        var logger = Substitute.For<ILogger<FetchController>>();

        _fetchService.TriggerReconciliationFetch(Arg.Any<ReconciliationFetchRequest>())
            .Returns((true, new SoapResult.Success(new XmlDocument())));

        _authenticationClient.GetElkContextAsync(Arg.Any<string>())
            .Returns(CreateStubElkContext());

        _controller = new FetchController(_authenticationClient, _fetchService, logger);
    }

    private void SetupFetchRequest(ReconciliationFetchRequest fetchRequest)
    {
        var json = JsonConvert.SerializeObject(fetchRequest);
        var context = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "FetchRequest", "true" } });
        ControllerTestHelper.SetRequestBody(context, json);
        _controller.ControllerContext = context;
    }

    [Fact]
    public async Task WhenRequestHasFetchHeader_It_FetchesPushUpdatesForTheGivenPACodeAndDateRange()
    {
        var fetchRequest = new ReconciliationFetchRequest
        {
            PACode = "good1",
            StartDateTime = DateTime.Parse("2020-02-02"),
            EndDateTime = DateTime.Parse("2020-03-03")
        };

        SetupFetchRequest(fetchRequest);

        var result = await _controller.Post();

        await _authenticationClient.Received(1).GetElkContextAsync(Arg.Any<string>());
        await _fetchService.Received(1).TriggerReconciliationFetch(
            Arg.Is<ReconciliationFetchRequest>(x => x.PACode == fetchRequest.PACode));
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(202, statusResult.StatusCode);
    }

    [Fact]
    public async Task WhenFetchRequestFailsAuthorization_It_ReturnsUnauthorized()
    {
        _authenticationClient.GetElkContextAsync(Arg.Any<string>())
            .Throws(new Exception("Unauthorized"));

        var fetchRequest = new ReconciliationFetchRequest
        {
            PACode = "good1",
            StartDateTime = DateTime.Parse("2020-02-02"),
            EndDateTime = DateTime.Parse("2020-03-03")
        };

        SetupFetchRequest(fetchRequest);

        var result = await _controller.Post();

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(401, statusResult.StatusCode);
    }

    [Fact]
    public async Task WhenFetchRequestFailsValidation_It_ReturnsBadRequest()
    {
        var fetchRequest = new ReconciliationFetchRequest
        {
            PACode = null!,
            StartDateTime = DateTime.Parse("2020-02-02"),
            EndDateTime = DateTime.Parse("2020-03-03")
        };

        SetupFetchRequest(fetchRequest);

        var result = await _controller.Post();

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(400, statusResult.StatusCode);
    }

    private static ElkContext CreateStubElkContext()
    {
        var identity = new ElkIdentity(Guid.NewGuid(), Guid.NewGuid());
        return new ElkContext(identity, identity, null!, null!);
    }
}
