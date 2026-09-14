using System.Collections;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim.Data
{
    public class ClaimTypesWithEmptyAppealCodes : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { ClaimTypes.VehicleCoverages, "" };
            yield return new object[] { ClaimTypes.VehicleCoverages, null };
            yield return new object[] { ClaimTypes.PreDeliveryInspection, "" };
            yield return new object[] { ClaimTypes.PreDeliveryInspection, null };
            yield return new object[] { ClaimTypes.Policy, "" };
            yield return new object[] { ClaimTypes.Policy, null };
            yield return new object[] { ClaimTypes.MisBuilt, "" };
            yield return new object[] { ClaimTypes.MisBuilt, null };
            yield return new object[] { ClaimTypes.ServiceParts, "" };
            yield return new object[] { ClaimTypes.ServiceParts, null };
            yield return new object[] { ClaimTypes.OverTheCounter, "" };
            yield return new object[] { ClaimTypes.OverTheCounter, null };
            yield return new object[] { ClaimTypes.Accessories, "" };
            yield return new object[] { ClaimTypes.Accessories, null };
            yield return new object[] { ClaimTypes.FieldServiceAction, "" };
            yield return new object[] { ClaimTypes.FieldServiceAction, null };
            yield return new object[] { ClaimTypes.FreeInspection, "" };
            yield return new object[] { ClaimTypes.FreeInspection, null };
            yield return new object[] { ClaimTypes.TransitDamage, "" };
            yield return new object[] { ClaimTypes.TransitDamage, null };
            yield return new object[] { ClaimTypes.Fleet, "" };
            yield return new object[] { ClaimTypes.Fleet, null };
            yield return new object[] { ClaimTypes.ExtendedServiceContracts, "" };
            yield return new object[] { ClaimTypes.ExtendedServiceContracts, null };
            yield return new object[] { ClaimTypes.RetailCoreReturn, "" };
            yield return new object[] { ClaimTypes.RetailCoreReturn, null };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
