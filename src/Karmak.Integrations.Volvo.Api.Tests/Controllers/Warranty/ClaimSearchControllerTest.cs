using Karmak.Integrations.Volvo.Api.Controllers.Warranty;
using Karmak.Integrations.Volvo.Api.Test.Helpers;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Karmak.Integrations.Volvo.Api.Test.Controllers.Warranty;

public class ClaimSearchControllerTest
{
    private readonly IClaimSearchService _claimSearchService = Substitute.For<IClaimSearchService>();
    private readonly ClaimSearchController _controller;

    public ClaimSearchControllerTest()
    {
        _controller = new ClaimSearchController(_claimSearchService)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext()
        };
    }

    [Fact]
    public async Task WhenPosted_It_DelegatesToTheSearchService()
    {
        var request = new ClaimSearchRequest { Keyword = "291" };

        await _controller.Search(request);

        await _claimSearchService.Received(1).Search(request);
    }

    [Fact]
    public async Task WhenSearched_ItReturns_TheResults()
    {
        var response = new ClaimSearchResponse
        {
            TotalCount = 137,
            Results = [new ClaimSearchResult { ClaimId = "claim-1" }]
        };

        _claimSearchService.Search(Arg.Any<ClaimSearchRequest>()).Returns(response);

        var result = await _controller.Search(new ClaimSearchRequest());

        Assert.Same(response, Assert.IsType<ObjectResult>(result).Value);
    }

    /// <summary>
    /// A search naming a field or operator that does not exist is the caller's to correct, so it
    /// comes back as a bad request carrying what was wrong rather than as a failure.
    /// </summary>
    [Fact]
    public async Task WhenTheSearchCannotBeRunAsAsked_ItReturns_400WithTheReason()
    {
        _claimSearchService.Search(Arg.Any<ClaimSearchRequest>())
            .Throws(new InvalidClaimSearchException("'jsonData' is not a field claims can be searched by."));

        var result = await _controller.Search(new ClaimSearchRequest());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("'jsonData' is not a field claims can be searched by.", badRequest.Value);
    }
}
