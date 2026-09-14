using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Storage;
using Karmak.Integrations.Volvo.Warranty.Validators;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Warranty.Consumers
{
    public class RepairOrderSavedConsumer : IConsumer<RepairOrderReceived>
    {
        private readonly ILogger<RepairOrderSavedConsumer> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly ISettingsProvider _settingsClient;
        private readonly WarrantyRepairOrderValidator _warrantyRepairOrderValidator;
        private readonly IRepairOrderToClaimMapper _mapper;
        private readonly IClaimsService _claimsService;
        private readonly IWarrantyTableClient _tableClient;
        private readonly IKarmakBlobClient _claimCheckClient;

        public RepairOrderSavedConsumer(
            ILogger<RepairOrderSavedConsumer> logger,
            IExtendedLoggingClient extendedLoggingClient,
            ISettingsProvider settingsClient,
            IWarrantyTableClient tableClient,
            WarrantyRepairOrderValidator warrantyRepairOrderValidator,
            IRepairOrderToClaimMapper mapper,
            IClaimsService claimsService,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _extendedLoggingClient = extendedLoggingClient ?? throw new ArgumentNullException(nameof(extendedLoggingClient));
            _settingsClient = settingsClient ?? throw new ArgumentNullException(nameof(settingsClient));
            _warrantyRepairOrderValidator = warrantyRepairOrderValidator ?? throw new ArgumentNullException(nameof(warrantyRepairOrderValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
            _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
            _claimCheckClient = claimCheckClient ?? throw new ArgumentNullException(nameof(claimCheckClient));
        }

        public async Task Consume(ConsumeContext<RepairOrderReceived> context)
        {
            var repairOrder = await _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(context.Message.BlobName);

            var repairOrderData = new Dictionary<string, string> {
                { "Repair Order Number",  repairOrder.RepairOrderNumber},
                { "Original Repair Order Number", repairOrder.OriginalRepairOrderNumber }
            };

            _logger.LogInformationWithMetadata($"Received {nameof(RepairOrderSnapshot)} event", repairOrderData);
            await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(repairOrder), nameof(RepairOrderSnapshot), VolvoLogContentType.JSON);

            _logger.LogInformation($"Fetching {nameof(InterfaceOptions)} settings");
            var settings = await InvokeOrDefault(() => _settingsClient.GetSettingsAsync(), new VolvoSettings { InterfaceOptions = new InterfaceOptions() });

            var validationResults = _warrantyRepairOrderValidator.Validate(CreateRepairOrderContext(repairOrder, settings.InterfaceOptions));
            if (WarrantyIsDisabled(settings.InterfaceOptions))
            {
                _logger.LogInformationWithMetadata("Warranty is disabled. Aborting warranty claim creation.", new Dictionary<string, string>(repairOrderData) {
                    { Contracts.Constants.TelemetryKeys.DealerCode, settings.InterfaceOptions.PaCode },
                    { Contracts.Constants.TelemetryKeys.KarmakAccountNumber, repairOrder.FusionIdentityInfo.AccountCode }
                });
            }
            else if (validationResults.IsValid)
            {
                _logger.LogInformationWithMetadata($"Creating a warranty claim from {nameof(RepairOrderSnapshot)}", repairOrderData);

                try
                {
                    var fusionIdentity = new FusionIdentity
                    {
                        AccountCode = repairOrder.FusionIdentityInfo.AccountCode,
                        BranchCode = repairOrder.FusionIdentityInfo.BranchCode,
                        Username = repairOrder.FusionIdentityInfo.Username
                    };

                    if (repairOrder.Tasks.Count > 1)
                    {
                        var claims = await CreateClaimsFromRepairOrderWithMultipleJobs(repairOrder, settings, fusionIdentity);
                        await CreateClaimCorrelationTableRecord(claims, settings);
                    }
                    else
                    {
                        var claim = await _claimsService
                            .Create(_mapper
                                .Map(repairOrder, settings,
                                    repairOrder.Addresses.FirstOrDefault(a => a.AddressType == AddressType.SHIP_TO && a.EntityType == ConstantSettings.OwningCustomerAddressEntityType).Region),
                                fusionIdentity);
                        await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(claim), "Created Claim", VolvoLogContentType.JSON);
                    }
                }
                catch (ClaimAlreadyExistsException ex)
                {
                    _logger.LogInformationWithMetadata(ex.Message, repairOrderData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
            else
            {
                var validationErrors = GetValidationResultErrorMessage(validationResults);
                _logger.LogInformationWithMetadata("Repair Order Snapshot failed Warranty's validation. Discontinuing further processing.", new Dictionary<string, string>(repairOrderData) {
                    { "RepairOrderValidationErrors", validationErrors }
                });
            }
        }

        private IEnumerable<Claim> CreateClaimJobsFromRepairOrder(RepairOrderSnapshot repairOrderSnapshot, VolvoSettings settings, string correlationId)
        {
            _logger.LogInformationWithMetadata("Creating claims from Repair Order with multiple tasks.",
                new Dictionary<string, string> {
                    { "ClaimCorrelationId", correlationId },
                    { "RepairOrderNumber", repairOrderSnapshot.RepairOrderNumber }
                });
            var claims = repairOrderSnapshot.Tasks.Select(task =>
            {
                var snapshot = repairOrderSnapshot;
                snapshot.Tasks = new List<RepairOrderTask> { { task } };
                var claim = _mapper.Map(snapshot, settings,
                    snapshot.Addresses.FirstOrDefault(a => a.AddressType == AddressType.SHIP_TO && a.EntityType == ConstantSettings.OwningCustomerAddressEntityType)?.Region);
                claim.CorrelationId = correlationId;
                return claim;
            });
            return claims;
        }

        private async Task<IEnumerable<Claim>> CreateClaimsFromRepairOrderWithMultipleJobs(RepairOrderSnapshot snapshot, VolvoSettings settings, FusionIdentity fusionIdentity)
        {
            var correlationId = Guid.NewGuid().ToString();
            var claims = CreateClaimJobsFromRepairOrder(snapshot, settings, correlationId);

            var createdClaims = new List<Claim>();
            foreach (var claim in claims)
            {
                var createdClaim = await _claimsService.Create(claim, fusionIdentity);
                createdClaims.Add(createdClaim);
            }

            return createdClaims;
        }

        private async Task CreateClaimCorrelationTableRecord(IEnumerable<Claim> claims, VolvoSettings settings)
        {
            var correlationEntity = new ClaimJobCorrelationTableEntity(settings.InterfaceOptions.PaCode, claims.FirstOrDefault().CorrelationId)
            {
                ClaimJobIds = claims.Select(x => x.Id),
                RepairOrderNumber = claims.FirstOrDefault().RepairOrder.SecondaryIdentifier
            };

            await _tableClient.CreateEntity(TableConstants.CLAIM_JOB_CORRELATION_TABLE, correlationEntity);
        }

        private bool WarrantyIsDisabled(InterfaceOptions interfaceOptions)
        {
            if (ReactAndWarrantyEnabledAreNull(interfaceOptions))
            {
                return false;
            }
            else
            {
                return (bool)(!interfaceOptions.WarrantyEnabled);
            }
        }

        private bool ReactAndWarrantyEnabledAreNull(InterfaceOptions interfaceOptions)
        {
            return !(interfaceOptions.ReactEnabled.HasValue || interfaceOptions.WarrantyEnabled.HasValue);
        }

        private async Task<T> InvokeOrDefault<T>(Func<Task<T>> getFunc, T defaultValue)
        {
            try
            {
                return await getFunc.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return defaultValue;
            }
        }

        private static ValidationContext<RepairOrderSnapshot> CreateRepairOrderContext(RepairOrderSnapshot repairOrder, InterfaceOptions settings) =>
            new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = {
                    [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] =
                        settings?.WarrantyCustomers ?? Array.Empty<string>()
                }
            };

        private string GetValidationResultErrorMessage(FluentValidation.Results.ValidationResult validationResults)
        {
            var validationErrorMessages = validationResults.Errors.Select(x => x.ErrorMessage);
            return string.Join(", ", validationErrorMessages);
        }
    }
}
