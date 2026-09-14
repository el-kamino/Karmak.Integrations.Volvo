using System.Xml;
using Karmak.Integrations.Volvo.Api.Controllers.Warranty;
using Karmak.Integrations.Volvo.Api.Test.Helpers;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Api.Test.Controllers.Warranty;

public class PushControllerTest
{
    private readonly IVolvoPushUpdateService _pushUpdateService;
    private readonly PushController _controller;

    private readonly XmlDocument _request = FakeDocument("Request");
    private readonly XmlDocument _response = FakeDocument("Response");

    public PushControllerTest()
    {
        _pushUpdateService = Substitute.For<IVolvoPushUpdateService>();
        var logger = Substitute.For<ILogger<PushController>>();

        _controller = new PushController(_pushUpdateService);
    }

    private void SetupPushRequest(string xmlContent)
    {
        var context = ControllerTestHelper.CreateControllerContext();
        ControllerTestHelper.SetRequestBody(context, xmlContent);
        _controller.ControllerContext = context;
    }

    [Fact]
    public async Task WhenRequestIsSuccessfullyProcessed_It_Returns200()
    {
        _pushUpdateService.HandleUpdate(Arg.Any<XmlDocument>())
            .Returns((true, _response));

        SetupPushRequest(_request.OuterXml);

        var result = await _controller.Post();

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(200, contentResult.StatusCode);
    }

    [Fact]
    public async Task WhenRequestFailsToProcess_It_Returns500()
    {
        _pushUpdateService.HandleUpdate(Arg.Any<XmlDocument>())
            .Returns((false, _response));

        SetupPushRequest(_request.OuterXml);

        var result = await _controller.Post();

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(500, contentResult.StatusCode);
    }

    private static XmlDocument FakeDocument(string messageId)
    {
        var document = new XmlDocument();
        document.LoadXml($@"
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Header xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
                    <wsa:MessageID>{messageId}</wsa:MessageID>
                </soap:Header>
            </soap:Envelope>");
        return document;
    }
}
