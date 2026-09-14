using Karmak.Integrations.Volvo.Api.Controllers.Warranty;
using Karmak.Integrations.Volvo.Api.Test.Helpers;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Karmak.Integrations.Volvo.Api.Test.Controllers.Warranty;

public class ClaimsControllerTest
{
    private readonly IClaimsService _claimsService;
    private readonly ClaimsController _controller;
    private readonly IOptions<WarrantyConfigurationOptions> _options;
    private readonly WarrantyConfigurationOptions _optionsValue;

    public ClaimsControllerTest()
    {
        _claimsService = Substitute.For<IClaimsService>();
        var logger = Substitute.For<ILogger<ClaimsController>>();
        _optionsValue = new WarrantyConfigurationOptions { IsMockEnabled = false };
        _options = Substitute.For<IOptions<WarrantyConfigurationOptions>>();
        _options.Value.Returns(_optionsValue);

        _claimsService.Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>())
            .Returns(ci => ci.Arg<Claim>());

        _claimsService.Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Any<FusionIdentity>())
            .Returns(ci => ci.Arg<Claim>());

        _claimsService.Find(Arg.Any<string>())
            .Returns((Claim?)null);

        _controller = new ClaimsController(_claimsService, logger, _options)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext()
        };
    }

    [Fact]
    public async Task WhenPosted_It_DelegatesToClaimsService()
    {
        var fakeClaim = FakeClaim.Generate();

        await _controller.Create(fakeClaim);

        await _claimsService.Received(1).Create(
            Arg.Is<Claim>(c => c.Id == fakeClaim.Id),
            Arg.Any<FusionIdentity>());
    }

    [Fact]
    public async Task WhenSuccessfullyPosted_ItReturns_With201CreatedAtRoute()
    {
        var result = await _controller.Create(FakeClaim.Generate());

        Assert.IsType<CreatedAtRouteResult>(result);
    }

    [Fact]
    public async Task WhenSuccessfullyPosted_ItReturns_WithTheCreatedClaimAndId()
    {
        var fakeClaim = FakeClaim.Generate();

        var result = await _controller.Create(fakeClaim);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
        var returnedClaim = Assert.IsType<Claim>(createdResult.Value);
        Assert.Equal(fakeClaim.Id, returnedClaim.Id);
    }

    [Fact]
    public async Task WhenPostingWithFusionIdentity_It_CreatesWithTheFusionUser()
    {
        const string username = "eddieVanHalen";
        const string accountCode = "12345";
        const string branchCode = "01";

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            accountCode: accountCode,
            branchCode: branchCode,
            username: username);

        await _controller.Create(FakeClaim.Generate());

        await _claimsService.Received(1).Create(
            Arg.Any<Claim>(),
            Arg.Is<FusionIdentity>(fi =>
                fi.Username == username
                && fi.AccountCode == accountCode
                && fi.BranchCode == branchCode));
    }

    [Fact]
    public async Task WhenClaimAlreadyExists_It_ThrowsClaimAlreadyExistsException()
    {
        _claimsService.Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>())
            .Throws(new ClaimAlreadyExistsException());

        await Assert.ThrowsAsync<ClaimAlreadyExistsException>(
            () => _controller.Create(FakeClaim.Generate()));
    }

    [Fact]
    public async Task WhenGetByIdSucceeds_ItReturns_WithThePersistedClaim()
    {
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(fakeClaim.Id).Returns(fakeClaim);

        var result = await _controller.Get(fakeClaim.Id);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var returnedClaim = Assert.IsType<Claim>(objectResult.Value);
        Assert.Equal(fakeClaim.Id, returnedClaim.Id);
    }

    [Fact]
    public async Task WhenGetByIdFailsToFind_ItReturns_NotFound()
    {
        _claimsService.Find(Arg.Any<string>()).Returns((Claim?)null);

        var result = await _controller.Get("bar");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task WhenGetByIdFindsDeletedClaim_ItReturns_NotFound()
    {
        var fakeClaim = FakeClaim.Generate();
        fakeClaim.IsDeleted = true;
        _claimsService.Find(fakeClaim.Id).Returns(fakeClaim);

        var result = await _controller.Get(fakeClaim.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task WhenGenerallyFailsToFind_It_ThrowsException()
    {
        _claimsService.Find(Arg.Any<string>())
            .Throws(new Exception("Something went wrong here..."));

        await Assert.ThrowsAsync<Exception>(() => _controller.Get("bar"));
    }

    [Fact]
    public async Task WhenUpdateIsOutdated_ItReturns_Conflict()
    {
        var fakeClaim = FakeClaim.Generate();

        _claimsService.Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Any<FusionIdentity>())
            .Returns((Claim?)null);

        var result = await _controller.Put(fakeClaim, fakeClaim.Id);

        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task WhenUpdateSucceeds_ItReturns_WithTheUpdatedPersistedClaim()
    {
        var fakeClaim = FakeClaim.Generate();

        var result = await _controller.Put(fakeClaim, fakeClaim.Id);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var returnedClaim = Assert.IsType<Claim>(objectResult.Value);
        Assert.Equal(fakeClaim.Id, returnedClaim.Id);
    }

    [Fact]
    public async Task WhenUpdatingWithFusionIdentity_It_UpdatesWithTheFusionUser()
    {
        var fakeClaim = FakeClaim.Generate();
        const string username = "tedNugent";
        const string accountCode = "12345";
        const string branchCode = "01";

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            accountCode: accountCode,
            branchCode: branchCode,
            username: username);

        await _controller.Put(fakeClaim, fakeClaim.Id);

        await _claimsService.Received(1).Update(
            Arg.Any<Claim>(),
            Arg.Any<string>(),
            Arg.Is<FusionIdentity>(fi =>
                fi.Username == username
                && fi.AccountCode == accountCode
                && fi.BranchCode == branchCode));
    }

    [Fact]
    public async Task WhenDeleteSucceeds_ItReturns_NoContent()
    {
        var fakeClaim = FakeClaim.Generate();

        var result = await _controller.Delete(fakeClaim.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task WhenClaimSubmittedWithMockEnabledAndUseMockHeader_It_DoesNotDelegateToClaimsService()
    {
        _optionsValue.IsMockEnabled = true;
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns(fakeClaim);

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "true" } });

        await _controller.Submit(fakeClaim.Id);

        await _claimsService.DidNotReceive().Submit(Arg.Any<Claim>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenClaimSubmittedWithMockEnabledAndUseMockHeader_ItReturns_Accepted()
    {
        _optionsValue.IsMockEnabled = true;
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns(fakeClaim);

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "true" } });

        var result = await _controller.Submit(fakeClaim.Id);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WhenClaimSubmittedWithMockEnabledAndInvalidUseMockHeader_It_DelegatesToClaimsService()
    {
        _optionsValue.IsMockEnabled = true;
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns(fakeClaim);

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "foo" } });

        await _controller.Submit(fakeClaim.Id);

        await _claimsService.Received(1).Submit(Arg.Any<Claim>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenClaimSubmittedWithMockEnabledAndMissingUseMockHeader_It_DelegatesToClaimsService()
    {
        _optionsValue.IsMockEnabled = true;
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns(fakeClaim);

        await _controller.Submit(fakeClaim.Id);

        await _claimsService.Received(1).Submit(Arg.Any<Claim>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenClaimSubmittedWithMockDisabledAndUseMockHeader_It_DelegatesToClaimsService()
    {
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns(fakeClaim);

        _controller.ControllerContext = ControllerTestHelper.CreateControllerContext(
            headers: new Dictionary<string, string> { { "UseMock", "true" } });

        await _controller.Submit(fakeClaim.Id);

        await _claimsService.Received(1).Submit(Arg.Any<Claim>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenSubmitSucceeds_ItReturns_Accepted()
    {
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns(fakeClaim);

        var result = await _controller.Submit(fakeClaim.Id);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WhenSubmitCannotFindTheClaim_ItReturns_NotFound()
    {
        var fakeClaim = FakeClaim.Generate();
        _claimsService.Find(Arg.Any<string>()).Returns((Claim?)null);

        var result = await _controller.Submit(fakeClaim.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task WhenStatusUpdated_It_DelegatesToClaimsService()
    {
        await _controller.UpdateStatus("01", ClaimStatus.New);

        await _claimsService.Received(1).UpdateStatus(
            Arg.Any<string>(), Arg.Any<ClaimStatus>(), Arg.Any<FusionIdentity>());
    }

    [Fact]
    public async Task WhenStatusUpdated_ItReturns_WithOkObjectResult()
    {
        var fakeClaim = FakeClaim.Generate();
        _claimsService.UpdateStatus(Arg.Any<string>(), Arg.Any<ClaimStatus>(), Arg.Any<FusionIdentity>())
            .Returns(fakeClaim);

        var result = await _controller.UpdateStatus(fakeClaim.Id, ClaimStatus.New);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task WhenStatusUpdated_ItReturns_WithTheUpdatedClaim()
    {
        var fakeClaim = FakeClaim.Generate();
        _claimsService.UpdateStatus(Arg.Any<string>(), Arg.Any<ClaimStatus>(), Arg.Any<FusionIdentity>())
            .Returns(fakeClaim);

        var result = await _controller.UpdateStatus(fakeClaim.Id, ClaimStatus.New);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var returnedClaim = Assert.IsType<Claim>(objectResult.Value);
        Assert.Equal(fakeClaim.Id, returnedClaim.Id);
    }
}
