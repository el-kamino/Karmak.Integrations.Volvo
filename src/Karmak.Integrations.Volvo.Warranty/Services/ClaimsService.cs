using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Persistence;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Storage;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using Microsoft.Extensions.Logging;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class ClaimsService : IClaimsService
    {
        private const string InvalidClaimContextMessageTemplate = "Invalid claim context: {0}";

        private readonly IClaimRepository _claimRepository;
        private readonly IBusControl _bus;
        private readonly ISettingsProvider _settingsClient;
        private readonly ILogger<ClaimsService> _logger;
        private readonly IWarrantyTableClient _tableClient;

        public ClaimsService(
            IClaimRepository repository,
            IBusControl bus,
            ISettingsProvider settingsClient,
            ILogger<ClaimsService> logger,
            IWarrantyTableClient tableClient)
        {
            _claimRepository = repository;
            _bus = bus;
            _logger = logger;
            _settingsClient = settingsClient ?? throw new ArgumentNullException(nameof(settingsClient));
            _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
        }

        public async Task<Claim> Create(Claim claim, FusionIdentity fusionIdentity)
        {
            if (string.IsNullOrEmpty(claim.CorrelationId))
            {
                await AssertOnlyOneClaim(claim);
            }
            else
            {
                await AssertOnlyOneCorrelatingClaim(claim);
            }

            claim.Id = Guid.NewGuid().ToString();
            SetDealerInfo(claim, fusionIdentity);
            UpdatePartIdentifiers(claim);
            SetAuditInfo(claim, fusionIdentity, true);

            var createdClaim = await _claimRepository.Create(claim);
            var settings = await _settingsClient.GetSettingsAsync();

            _logger.LogInformationWithMetadata("Claim created.", new Dictionary<string, string> {
                { "Claim.Id", createdClaim.Id },
                { "Claim.Identifier", createdClaim.Identifier },
                { TelemetryKeys.DealerCode, settings.InterfaceOptions.PaCode},
                { TelemetryKeys.KarmakAccountNumber, createdClaim?.Dealer?.Account?.Code},
                { TelemetryKeys.OEM, TelemetryValues.Volvo },
                { TelemetryKeys.BusinessProcess, "Claim Created" },
                { TelemetryKeys.Module, TelemetryValues.Warranty },
                { "ClaimJobCorrelationId", claim.CorrelationId }
            });

            return createdClaim;
        }

        public async Task<Claim> Find(string id) =>
            await _claimRepository.Find(GetInstanceIdentifier(), id);

        public async Task<List<Claim>> FindByRepairOrderId(string repairOrderId)
        {
            var branch = ImplicitElkContext.Current.ApplicationContext.Branch?.ToString();

            var results = await _claimRepository.FindActiveByWarrantyRepairOrder(GetInstanceIdentifier(), branch, repairOrderId);

            return results.ToList();
        }

        public async Task Delete(string id, FusionIdentity fusionIdentity)
        {
            var claim = await _claimRepository.Find(GetInstanceIdentifier(), id);

            if (string.IsNullOrEmpty(claim.CorrelationId))
            {
                await DeleteClaim(claim, fusionIdentity);
            }
            else
            {
                await DeleteAllClaimJobs(claim, fusionIdentity);
            }
        }

        private async Task DeleteClaim(Claim claim, FusionIdentity fusionIdentity)
        {
            claim.IsDeleted = true;
            SetAuditInfo(claim, fusionIdentity);

            await _claimRepository.Update(claim);
        }

        private async Task DeleteAllClaimJobs(Claim claim, FusionIdentity fusionIdentity)
        {
            var settings = await _settingsClient.GetSettingsAsync();
            var record = await _tableClient.GetEntity<ClaimJobCorrelationTableEntity>(TableConstants.CLAIM_JOB_CORRELATION_TABLE, settings.InterfaceOptions.PaCode, claim.CorrelationId);

            foreach (var claimJobId in record.ClaimJobIds)
            {
                var claimJob = await _claimRepository.Find(GetInstanceIdentifier(), claimJobId);
                await DeleteClaim(claimJob, fusionIdentity);
            }
        }

        public async Task Submit(Claim message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformationWithMetadata("Submitting Claim", new Dictionary<string, string> {
                { "Claim.Id", message.Id },
                { "Correlation.Id", message.CorrelationId }
            });

            SubmitClaimPayload payload = await BuildSubmitClaimPayload(message);

            await _bus.Publish(payload, cancellationToken);
        }

        private async Task<SubmitClaimPayload> BuildSubmitClaimPayload(Claim claim)
        {
            if (string.IsNullOrEmpty(claim.CorrelationId) || claim.RepairOrder == null)
            {
                return new SubmitClaimPayload(claim);
            }

            var results = await _claimRepository.FindActiveInCorrelation(
                GetInstanceIdentifier(), GetRepairOrderKey(claim), claim.CorrelationId);

            return new SubmitClaimPayload(results);
        }

        public async Task<Claim> Update(Claim claim, string id, FusionIdentity fusionIdentity)
        {
            var originalClaim = await _claimRepository.Find(GetInstanceIdentifier(), id);

            if (UpdateIsOutdated(originalClaim.UpdatedDateTime, claim.UpdatedDateTime))
                return null;

            RetainStoredValues(claim, originalClaim);
            UpdatePartIdentifiers(claim);
            SetAuditInfo(claim, fusionIdentity);

            var settings = await _settingsClient.GetSettingsAsync();
            GetUpdatedOEMUserID(claim, settings);

            var updatedClaim = await _claimRepository.Update(claim);
            return updatedClaim;
        }

        public async Task<Claim> UpdateStatus(string id, ClaimStatus status, FusionIdentity fusionIdentity)
        {
            var originalClaim = await _claimRepository.Find(GetInstanceIdentifier(), id);

            originalClaim.Status = status;
            SetAuditInfo(originalClaim, fusionIdentity);

            var updatedClaim = await _claimRepository.Update(originalClaim);
            return updatedClaim;
        }

        public async Task<Claim> AddToUpdateSnapshotHistory(string id, UpdateSnapshot updateSnapshot, FusionIdentity fusionIdentity)
        {
            var originalClaim = await _claimRepository.Find(GetInstanceIdentifier(), id);

            if (UpdateIsNotUnique(originalClaim, updateSnapshot))
            {
                return originalClaim;
            }

            var editedClaim = ApplyUpdateSnapshot(originalClaim, updateSnapshot);
            SetAuditInfo(editedClaim, fusionIdentity);

            var updatedClaim = await _claimRepository.Update(editedClaim);
            return updatedClaim;
        }

        private async Task AssertOnlyOneClaim(IClaim claim)
        {
            if (claim.RepairOrder == null) { return; }

            var results = await _claimRepository.FindActiveByRepairOrder(
                GetInstanceIdentifier(), GetRepairOrderKey(claim));

            if (results.Any())
            {
                throw new ClaimAlreadyExistsException($"An active claim already exists matching the criteria: {results.FirstOrDefault()?.Id}");
            }
        }

        private async Task AssertOnlyOneCorrelatingClaim(IClaim claim)
        {
            if (claim.RepairOrder == null) { return; }

            var results = await _claimRepository.FindActiveOutsideCorrelation(
                GetInstanceIdentifier(), GetRepairOrderKey(claim), claim.CorrelationId);

            if (results.Any())
            {
                throw new ClaimAlreadyExistsException($"An active claim without a correaltion id already exists matching the criteria: {results.FirstOrDefault()?.Id}");
            }
        }

        private static RepairOrderKey GetRepairOrderKey(IClaim claim) =>
            new(GetBranchIdentifier(), claim.RepairOrder.Identifier, claim.RepairOrder.SecondaryIdentifier);

        private static bool UpdateIsNotUnique(Claim claim, UpdateSnapshot updateSnapshot)
        {
            var matchingUpdates = claim.UpdateSnapshots.Where(x => x.OemId == updateSnapshot.OemId);
            return matchingUpdates.Any(x => x.IsDuplicate(updateSnapshot));
        }

        private Claim ApplyUpdateSnapshot(Claim claim, UpdateSnapshot updateSnapshot)
        {
            claim.Status = new ClaimStatus(updateSnapshot.Status.Description, updateSnapshot.Status.Code);
            claim.UpdateSnapshots.Add(updateSnapshot);
            return claim;
        }

        private static void RetainStoredValues(Claim claim, IClaim originalClaim)
        {
            claim.Id = originalClaim.Id;
            claim.Dealer = originalClaim.Dealer;
            claim.CreatedBy = originalClaim.CreatedBy;
            claim.CreatedDateTime = originalClaim.CreatedDateTime;
        }

        private static void SetDealerInfo(Claim claim, FusionIdentity fusionIdentity)
        {
            claim.Dealer = new Dealer
            {
                Account = new Account
                {
                    Identifier = GetAccountIdentifier(),
                    Code = fusionIdentity.AccountCode
                },
                Branch = new Branch
                {
                    Identifier = GetBranchIdentifier(),
                    Code = fusionIdentity.BranchCode
                },
                InstanceIdentifier = GetInstanceIdentifier()
            };
        }

        private void SetAuditInfo(Claim claim, FusionIdentity fusionIdentity, bool isNew = false)
        {
            if (isNew)
            {
                claim.CreatedDateTime = DateTime.UtcNow;
                claim.CreatedBy = fusionIdentity.Username;
            }

            claim.UpdatedDateTime = DateTime.UtcNow;
            claim.UpdatedBy = fusionIdentity.Username;
        }

        private static string GetAccountIdentifier()
        {
            return ImplicitElkContext.Current?.Identity?.Account.ToString() ??
                throw new InvalidClaimContextException(
                    string.Format(InvalidClaimContextMessageTemplate, "Elk Account is missing."));
        }

        private static string GetBranchIdentifier()
        {
            return ImplicitElkContext.Current.ApplicationContext?.Branch?.ToString() ??
                   throw new InvalidClaimContextException(
                       string.Format(InvalidClaimContextMessageTemplate, "Elk Branch is missing."));
        }

        private static string GetInstanceIdentifier()
        {
            return ImplicitElkContext.Current.ApplicationContext?.Instance?.ToString() ??
                   throw new InvalidClaimContextException(
                       string.Format(InvalidClaimContextMessageTemplate, "Elk Instance is missing."));
        }

        private static void UpdatePartIdentifiers(Claim claim)
        {
            foreach (var job in claim.RepairOrder.Jobs)
            {
                foreach (var part in job.PartExpenses)
                {
                    part.Identifier = $"{part.Prefix}{part.Number}{part.Suffix}";
                }
            }
        }

        private static bool UpdateIsOutdated(DateTimeOffset original, DateTimeOffset update)
        {
            var year = original.UtcDateTime.Year == update.UtcDateTime.Year;
            var month = original.UtcDateTime.Month == update.UtcDateTime.Month;
            var day = original.UtcDateTime.Day == update.UtcDateTime.Day;
            var hour = original.UtcDateTime.Hour == update.UtcDateTime.Hour;
            var minute = original.UtcDateTime.Minute == update.UtcDateTime.Minute;
            var second = original.UtcDateTime.Second == update.UtcDateTime.Second;
            var millisecond = original.UtcDateTime.Millisecond == update.UtcDateTime.Millisecond;
            return !(year && month && day && hour && minute && second && millisecond);
        }

        private static void GetUpdatedOEMUserID(Claim claim, VolvoSettings settings)
        {
            var oemUserMappings = (settings?.InterfaceOptions?.OemUserMappings
                ?? System.Collections.Immutable.ImmutableDictionary<string, string>.Empty)
                .ToImmutableDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            var strippedUsername = claim.RepairOrder.Advisor.Username?.Replace(".", string.Empty) ?? string.Empty;

            claim.RepairOrder.Advisor.Identifier = oemUserMappings.ContainsKey(strippedUsername)
                ? oemUserMappings[strippedUsername]
                : claim.RepairOrder.Advisor.Username;
        }
    }
}
