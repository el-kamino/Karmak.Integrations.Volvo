using Karmak.Integrations.Volvo.Api.Controllers.Warranty;
using Karmak.Integrations.Volvo.Api.Test.Helpers;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Api.Test.Controllers.Warranty;

public class StandardCodesControllerTest
{
    private readonly IStandardCodesService _standardCodesService;
    private readonly StandardCodesController _controller;
    private readonly WarrantyConfigurationOptions _optionsValue;

    public StandardCodesControllerTest()
    {
        _standardCodesService = Substitute.For<IStandardCodesService>();
        _optionsValue = new WarrantyConfigurationOptions { IsMockEnabled = false };
        var options = Substitute.For<IOptions<WarrantyConfigurationOptions>>();
        options.Value.Returns(_optionsValue);

        _standardCodesService.FetchCodesAsync()
            .Returns(new List<StandardCode>());

        _controller = new StandardCodesController(_standardCodesService, options)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext()
        };
    }

    [Fact]
    public async Task WhenGotWithMockEnabledAndUseMockHeader_It_ReturnsMockData()
    {
        _optionsValue.IsMockEnabled = true;

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "true" } });

        var result = await _controller.FetchCodes();

        var objectResult = Assert.IsType<ObjectResult>(result);
        var codes = Assert.IsAssignableFrom<StandardCode[]>(objectResult.Value);
        Assert.Single(codes);
        Assert.Equal("00", codes[0].Code);

        await _standardCodesService.DidNotReceive().FetchCodesAsync();
    }

    [Fact]
    public async Task WhenGotWithMockEnabledAndInvalidUseMockHeader_It_DelegatesToStandardCodesService()
    {
        _optionsValue.IsMockEnabled = true;

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "foobar" } });

        await _controller.FetchCodes();

        await _standardCodesService.Received(1).FetchCodesAsync();
    }

    [Fact]
    public async Task WhenGotWithMockEnabledAndMissingUseMockHeader_It_DelegatesToStandardCodesService()
    {
        _optionsValue.IsMockEnabled = true;

        await _controller.FetchCodes();

        await _standardCodesService.Received(1).FetchCodesAsync();
    }

    [Fact]
    public async Task WhenGotWithMockDisabledAndUseMockHeader_It_DelegatesToStandardCodesService()
    {
        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "true" } });

        await _controller.FetchCodes();

        await _standardCodesService.Received(1).FetchCodesAsync();
    }

    [Fact]
    public async Task WhenGotWithMockDisabledAndMissingUseMockHeader_It_DelegatesToStandardCodesService()
    {
        await _controller.FetchCodes();

        await _standardCodesService.Received(1).FetchCodesAsync();
    }

    [Fact]
    public async Task WhenSuccessfullyGot_ItReturns_WithObjectResult()
    {
        var result = await _controller.FetchCodes();

        Assert.IsType<ObjectResult>(result);
    }

    [Fact]
    public async Task WhenSuccessfullyGot_ItReturns_WithAListOfStandardCodes()
    {
        var standardCodes = new List<StandardCode>
        {
            new StandardCode
            {
                Code = "test.code",
                CodeType = "test.type",
                Description = "test.description"
            }
        };

        _standardCodesService.FetchCodesAsync().Returns(standardCodes);

        var result = await _controller.FetchCodes();

        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<StandardCode>>(objectResult.Value);
        var responseList = response.ToList();
        Assert.Single(responseList);
        Assert.Equal("test.code", responseList[0].Code);
        Assert.Equal("test.type", responseList[0].CodeType);
        Assert.Equal("test.description", responseList[0].Description);
    }
}
