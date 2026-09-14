using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Karmak.Integrations.Volvo.Warranty.Mapping;

namespace Karmak.Integrations.Volvo.Warranty.Persistence
{
    /// <summary>
    /// Serves <see cref="IClaimRepository"/> from Azure Sql. The claim itself lives in the
    /// row's json column; the lookups run against the columns promoted alongside it.
    /// </summary>
    public class SqlClaimRepository : IClaimRepository
    {
        private readonly IWarrantyDataRepository _repository;
        private readonly IWarrantyClaimEntityMapper _mapper;

        public SqlClaimRepository(IWarrantyDataRepository repository, IWarrantyClaimEntityMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //Create and update are the same write: the row is keyed on the claim's own id, so the
        //store does not need to know which one the caller thinks it is doing.
        public Task<Claim> Create(Claim claim) => Save(claim);

        public Task<Claim> Update(Claim claim) => Save(claim);

        public async Task<Claim> Find(string instanceIdentifier, string id)
        {
            var entity = await _repository.FindAsync(instanceIdentifier, id);

            return _mapper.ToClaim(entity);
        }

        public Task<IEnumerable<Claim>> FindActiveByWarrantyRepairOrder(
            string instanceIdentifier,
            string branchIdentifier,
            string warrantyRepairOrderIdentifier)
        {
            return Query(new WarrantyClaimQuery
            {
                InstanceIdentifier = instanceIdentifier,
                BranchIdentifier = branchIdentifier,
                WarrantyRepairOrderIdentifier = warrantyRepairOrderIdentifier,
                IsDeleted = false
            });
        }

        public Task<IEnumerable<Claim>> FindActiveByRepairOrder(string instanceIdentifier, RepairOrderKey repairOrder)
        {
            return Query(BuildQuery(instanceIdentifier, repairOrder));
        }

        public Task<IEnumerable<Claim>> FindActiveInCorrelation(
            string instanceIdentifier,
            RepairOrderKey repairOrder,
            string correlationId)
        {
            var query = BuildQuery(instanceIdentifier, repairOrder);
            query.CorrelationId = correlationId;

            return Query(query);
        }

        public Task<IEnumerable<Claim>> FindActiveOutsideCorrelation(
            string instanceIdentifier,
            RepairOrderKey repairOrder,
            string correlationId)
        {
            var query = BuildQuery(instanceIdentifier, repairOrder);
            query.CorrelationId = correlationId;
            query.ExcludeCorrelationId = true;

            return Query(query);
        }

        public async Task<ClaimSearchResponse> Search(WarrantyClaimSearchQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var page = await _repository.SearchAsync(query);

            return new ClaimSearchResponse
            {
                TotalCount = page.TotalCount,
                Skip = query.Skip,
                Top = query.Take,
                Results = page.Items.Select(_mapper.ToSearchResult).ToList()
            };
        }

        private async Task<Claim> Save(Claim claim)
        {
            ArgumentNullException.ThrowIfNull(claim);

            var entity = _mapper.ToEntity(claim);

            if (string.IsNullOrWhiteSpace(entity.InstanceIdentifier))
            {
                throw new InvalidClaimContextException(
                    $"Invalid claim context: claim '{claim.Id}' has no dealer instance identifier to store it under.");
            }

            await _repository.UpsertAsync(entity);

            return claim;
        }

        private static WarrantyClaimQuery BuildQuery(string instanceIdentifier, RepairOrderKey repairOrder)
        {
            ArgumentNullException.ThrowIfNull(repairOrder);

            return new WarrantyClaimQuery
            {
                InstanceIdentifier = instanceIdentifier,
                BranchIdentifier = repairOrder.BranchIdentifier,
                RepairOrderIdentifier = repairOrder.RepairOrderIdentifier,
                WarrantyRepairOrderIdentifier = repairOrder.WarrantyRepairOrderIdentifier,
                IsDeleted = false
            };
        }

        private async Task<IEnumerable<Claim>> Query(WarrantyClaimQuery query)
        {
            var entities = await _repository.QueryAsync(query);

            return entities
                .Select(_mapper.ToClaim)
                .Where(claim => claim != null)
                .ToList();
        }
    }
}
