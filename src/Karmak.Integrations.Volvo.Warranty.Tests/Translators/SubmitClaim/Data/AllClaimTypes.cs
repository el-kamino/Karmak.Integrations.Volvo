using System.Collections;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim.Data
{
    public class AllClaimTypes : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { ClaimTypes.VehicleCoverages };
            yield return new object[] { ClaimTypes.PreDeliveryInspection };
            yield return new object[] { ClaimTypes.Policy };
            yield return new object[] { ClaimTypes.MisBuilt };
            yield return new object[] { ClaimTypes.ServiceParts };
            yield return new object[] { ClaimTypes.OverTheCounter };
            yield return new object[] { ClaimTypes.Accessories };
            yield return new object[] { ClaimTypes.FieldServiceAction };
            yield return new object[] { ClaimTypes.FreeInspection };
            yield return new object[] { ClaimTypes.TransitDamage };
            yield return new object[] { ClaimTypes.Fleet };
            yield return new object[] { ClaimTypes.ExtendedServiceContracts };
            yield return new object[] { ClaimTypes.RetailCoreReturn };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
