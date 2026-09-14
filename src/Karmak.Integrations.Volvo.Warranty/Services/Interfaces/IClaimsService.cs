using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IClaimsService
    {
        Task<Claim> Create(Claim claim, FusionIdentity fusionIdentity);
        Task<Claim> Find(string id);
        Task<List<Claim>> FindByRepairOrderId(string repairOrderId);
        Task<Claim> Update(Claim claim, string id, FusionIdentity fusionIdentity);
        Task Delete(string id, FusionIdentity fusionIdentity);
        Task Submit(Claim message, CancellationToken cancellationToken = default);
        Task<Claim> UpdateStatus(string id, ClaimStatus status, FusionIdentity fusionIdentity);
        Task<Claim> AddToUpdateSnapshotHistory(string id, UpdateSnapshot updateSnapshot, FusionIdentity fusionIdentity);
    }
}
