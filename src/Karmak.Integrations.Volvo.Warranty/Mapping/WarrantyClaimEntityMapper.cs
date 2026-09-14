using System;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    /// <summary>
    /// The promoted columns are sourced exactly as <see cref="ClaimToClaimDocumentMapper"/>
    /// sources the equivalent search document fields, so a claim searched in sql and the same
    /// claim searched in the index agree.
    /// </summary>
    /// <remarks>
    /// Newtonsoft, not System.Text.Json: the claim graph relies on Newtonsoft's string enum
    /// converters, a remapped id property, and types such as
    /// <see cref="Contracts.ClaimStatus"/> whose setters are not public. It is also the
    /// serializer cosmos used, which keeps migrated documents byte-comparable.
    /// </remarks>
    public class WarrantyClaimEntityMapper : IWarrantyClaimEntityMapper
    {
        public WarrantyClaimEntity ToEntity(Claim claim)
        {
            ArgumentNullException.ThrowIfNull(claim);

            return new WarrantyClaimEntity
            {
                ClaimId = claim.Id,
                InstanceIdentifier = claim.Dealer?.InstanceIdentifier,
                BranchIdentifier = claim.Dealer?.Branch?.Identifier,
                BranchCode = claim.Dealer?.Branch?.Code,
                CorrelationId = claim.CorrelationId,
                IsDeleted = claim.IsDeleted,

                CausalPartIdentifier = claim.CausalPartIdentifier,
                ClaimIdentifier = claim.Identifier,
                CompanyName = claim.Customer?.CompanyName,
                ClaimTotal = claim.Total?.Value,
                RepairOrderCompletedDate = claim.RepairOrder?.CompletedDate,
                RepairOrderOpenedDate = claim.RepairOrder?.OpenedDate,
                CustomerIdentifier = claim.Customer?.Identifier,
                InvoiceIdentifier = claim.RepairOrder?.InvoiceIdentifier,
                Oem = claim.Oem?.Name,
                RepairOrderIdentifier = claim.RepairOrder?.Identifier,
                WarrantyRepairOrderIdentifier = claim.RepairOrder?.SecondaryIdentifier,
                ClaimStatus = claim.Status?.Value,
                VehicleIdentifier = claim.Unit?.Vehicle?.Identifier,

                JsonData = JsonConvert.SerializeObject(claim)
            };
        }

        public Claim ToClaim(WarrantyClaimEntity entity)
        {
            if (entity?.JsonData == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Claim>(entity.JsonData);
        }

        public ClaimSearchResult ToSearchResult(WarrantyClaimSummary summary)
        {
            if (summary == null)
            {
                return null;
            }

            return new ClaimSearchResult
            {
                ClaimId = summary.ClaimId,
                BranchIdentifier = summary.BranchIdentifier,
                BranchCode = summary.BranchCode,
                CorrelationId = summary.CorrelationId,
                IsDeleted = summary.IsDeleted,

                CausalPartIdentifier = summary.CausalPartIdentifier,
                ClaimIdentifier = summary.ClaimIdentifier,
                CompanyName = summary.CompanyName,
                ClaimTotal = summary.ClaimTotal,
                RepairOrderCompletedDate = summary.RepairOrderCompletedDate,
                RepairOrderOpenedDate = summary.RepairOrderOpenedDate,
                CustomerIdentifier = summary.CustomerIdentifier,
                InvoiceIdentifier = summary.InvoiceIdentifier,
                Oem = summary.Oem,
                RepairOrderIdentifier = summary.RepairOrderIdentifier,
                WarrantyRepairOrderIdentifier = summary.WarrantyRepairOrderIdentifier,
                ClaimStatus = summary.ClaimStatus,
                VehicleIdentifier = summary.VehicleIdentifier,

                CreatedOn = summary.CreatedOn,
                UpdatedOn = summary.UpdatedOn
            };
        }
    }
}
