using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.Warranty.Consumers;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Storage;
using Karmak.Integrations.Volvo.Warranty.Validators;
using MassTransit;
using MassTransit.Context;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Consumers
{
    public class RepairOrderSavedConsumerTest
    {
        private readonly ILogger<RepairOrderSavedConsumer> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly ISettingsProvider _settingsClient;
        private readonly IWarrantyTableClient _tableClient;
        private readonly IClaimsService _greatClaimsService;
        private readonly IClaimsService _poorClaimsService;
        private readonly IRepairOrderToClaimMapper _mapper;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly WarrantyRepairOrderValidator _warrantyRepairOrderValidator;
        private readonly MessageConsumeContext<RepairOrderReceived> _fakeMessageConsumeContext;
        private readonly ConsumeContext _consumeContext;
        private readonly RepairOrderSnapshot _fakeRepairOrderSnapshot;
        private readonly VolvoSettings _fakeSettings;

        public RepairOrderSavedConsumerTest()
        {
            _consumeContext = Substitute.For<ConsumeContext>();
            _logger = Substitute.For<ILogger<RepairOrderSavedConsumer>>();
            _extendedLoggingClient = Substitute.For<IExtendedLoggingClient>();
            _settingsClient = Substitute.For<ISettingsProvider>();
            _tableClient = Substitute.For<IWarrantyTableClient>();
            _greatClaimsService = Substitute.For<IClaimsService>();
            _poorClaimsService = Substitute.For<IClaimsService>();
            _mapper = Substitute.For<IRepairOrderToClaimMapper>();
            _claimCheckClient = Substitute.For<IKarmakBlobClient>();
            _warrantyRepairOrderValidator = new WarrantyRepairOrderValidator();

            _fakeSettings = AutoFaker.Generate<VolvoSettings>(builder => builder.WithRecursiveDepth(3));
            _fakeSettings.InterfaceOptions.WarrantyEnabled = true;
            _fakeSettings.InterfaceOptions.ReactEnabled = false;

            _fakeRepairOrderSnapshot = CreateValidRepairOrderSnapshot(_fakeSettings);

            var fakeMessage = new RepairOrderReceived { BlobName = "test-blob", RepairOrderId = "RO-123" };
            _fakeMessageConsumeContext = new MessageConsumeContext<RepairOrderReceived>(_consumeContext, fakeMessage);

            _consumeContext.Headers
                .Returns(Substitute.For<Headers>());

            _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(Arg.Any<string>())
                .Returns(_fakeRepairOrderSnapshot);

            _settingsClient.GetSettingsAsync()
                .Returns(_fakeSettings);

            _poorClaimsService.Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>())
                .Throws<Exception>();

            _greatClaimsService.Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>())
                .Returns(callInfo => callInfo.ArgAt<Claim>(0));

            _mapper.Map(Arg.Any<RepairOrderSnapshot>(), Arg.Any<VolvoSettings>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var repairOrder = (RepairOrderSnapshot)callInfo[0];
                    var claim = AutoFaker.Generate<Claim>();
                    if (repairOrder.Tasks.Count() < 2)
                        claim.CorrelationId = null;
                    return claim;
                });

        }

        [Fact]
        public void WhenCtorParametersAreNull_It_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(null, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, null, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, null, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, null, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, null, _mapper, _greatClaimsService, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, null, _greatClaimsService, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, null, _claimCheckClient); });
            Assert.Throws<ArgumentNullException>(() => { new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, null); });
        }

        [Fact]
        public async Task WhenConsumingAMessage_It_FetchesSettings()
        {
            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _settingsClient
                .Received(1).GetSettingsAsync();
        }

        [Fact]
        public async Task WhenWarrantyIsDisabled_ItDoesNot_CreateAClaim()
        {
            var stubSettings = AutoFaker.Generate<VolvoSettings>();
            stubSettings.InterfaceOptions.WarrantyEnabled = false;

            _settingsClient.GetSettingsAsync()
                .Returns(stubSettings);

            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _greatClaimsService
                .DidNotReceive().Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>());
        }

        [Fact]
        public async Task WhenWarrantyAndReactFlagsAreNull_It_WillCreateClaims()
        {
            var stubSettings = AutoFaker.Generate<VolvoSettings>();
            stubSettings.InterfaceOptions.WarrantyEnabled = null;
            stubSettings.InterfaceOptions.ReactEnabled = null;
            stubSettings.InterfaceOptions.WarrantyCustomers = new[] { "WARRANTY_CUSTOMER" };

            _settingsClient.GetSettingsAsync()
                .Returns(stubSettings);

            var repairOrderSnapshot = CreateValidRepairOrderSnapshot(stubSettings);
            repairOrderSnapshot.Tasks = new List<RepairOrderTask> {
                AutoFaker.Generate<RepairOrderTask>(),
                AutoFaker.Generate<RepairOrderTask>(),
                AutoFaker.Generate<RepairOrderTask>()
            };

            _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(Arg.Any<string>())
                .Returns(repairOrderSnapshot);

            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _greatClaimsService
                .Received(3).Create(Arg.Is<Claim>(x => x.CorrelationId != null), Arg.Any<FusionIdentity>());
        }

        [Fact]
        public async Task WhenRepairOrderHasMultipleTasks_It_CreatesATableEntryToCorrelateTheCreatedClaims()
        {
            var repairOrderSnapshot = CreateValidRepairOrderSnapshot(_fakeSettings);
            repairOrderSnapshot.Tasks = new List<RepairOrderTask> {
                AutoFaker.Generate<RepairOrderTask>(),
                AutoFaker.Generate<RepairOrderTask>(),
                AutoFaker.Generate<RepairOrderTask>()
            };

            _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(Arg.Any<string>())
                .Returns(repairOrderSnapshot);

            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _tableClient.Received(1).CreateEntity(TableConstants.CLAIM_JOB_CORRELATION_TABLE, Arg.Is<ClaimJobCorrelationTableEntity>(x => x.ClaimJobIds.Count() == 3));
        }

        [Fact]
        public async Task WhenRepairOrderSnapshot_HasASingleTask_OneClaimWillBeCreated()
        {
            var repairOrderSnapshot = CreateValidRepairOrderSnapshot(_fakeSettings);
            repairOrderSnapshot.Tasks = new List<RepairOrderTask> { AutoFaker.Generate<RepairOrderTask>() };

            _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(Arg.Any<string>())
                .Returns(repairOrderSnapshot);

            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _greatClaimsService
                .Received(1).Create(Arg.Is<Claim>(x => x.CorrelationId == null), Arg.Any<FusionIdentity>());
        }

        [Fact]
        public async Task WhenRepairOrderIsValidForWarrantyAndWarrantyIsEnabled_It_CreatesAClaim()
        {
            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _greatClaimsService
                .Received().Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>());
        }

        [Fact]
        public async Task WhenRepairOrderIsNotValidForWarranty_It_DoesNotCreateAClaim()
        {
            var invalidSnapshot = CreateValidRepairOrderSnapshot(_fakeSettings);
            invalidSnapshot.RepairOrderNumber = null;

            _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(Arg.Any<string>())
                .Returns(invalidSnapshot);

            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _greatClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            await _greatClaimsService
                .DidNotReceive().Create(Arg.Any<Claim>(), Arg.Any<FusionIdentity>());
        }

        [Fact]
        public async Task WhenFailingToCreate_It_DoesNotThrow()
        {
            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _poorClaimsService, _claimCheckClient);

            var exception = await Record.ExceptionAsync(() => consumer.Consume(_fakeMessageConsumeContext));

            Assert.Null(exception);
        }

        [Fact]
        public async Task WhenFailingToCreate_It_TracesTheException()
        {
            var consumer = new RepairOrderSavedConsumer(_logger, _extendedLoggingClient, _settingsClient, _tableClient, _warrantyRepairOrderValidator, _mapper, _poorClaimsService, _claimCheckClient);

            await consumer.Consume(_fakeMessageConsumeContext);

            _logger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        private static RepairOrderSnapshot CreateValidRepairOrderSnapshot(VolvoSettings settings)
        {
            var warrantyCustomerKey = "WARRANTY_CUSTOMER";
            settings.InterfaceOptions.WarrantyCustomers = new[] { warrantyCustomerKey };

            var snapshot = AutoFaker.Generate<RepairOrderSnapshot>(builder => builder.WithRecursiveDepth(3));
            snapshot.RepairOrderNumber = "RO-12345";
            snapshot.InvoiceDate = DateTime.Now;
            snapshot.BillingCustomer = new React.Contracts.Common.Customer { CustomerKey = warrantyCustomerKey };
            snapshot.FusionIdentityInfo = new FusionIdentityInfo
            {
                AccountCode = "ACC001",
                BranchCode = "BR001",
                Username = "testuser"
            };
            snapshot.Addresses = new List<React.Contracts.Common.Address> {
                new React.Contracts.Common.Address {
                    AddressType = AddressType.SHIP_TO,
                    EntityType = ConstantSettings.OwningCustomerAddressEntityType,
                    Region = "US"
                }
            };
            snapshot.Tasks = new List<RepairOrderTask> { AutoFaker.Generate<RepairOrderTask>() };
            return snapshot;
        }
    }
}
