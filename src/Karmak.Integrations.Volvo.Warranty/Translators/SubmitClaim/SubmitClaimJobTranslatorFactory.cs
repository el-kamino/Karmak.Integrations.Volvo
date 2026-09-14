using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public static class SubmitClaimJobTranslatorFactory
    {
        public static ITranslatable<SubmitClaimJobTranslatorArguments, JobExtended> Create() =>
            new TranslatorDelegate<SubmitClaimJobTranslatorArguments, JobExtended>(
                new Dictionary<Func<SubmitClaimJobTranslatorArguments, bool>, ITranslatable<SubmitClaimJobTranslatorArguments, JobExtended>> {
                    { ByClaimType(ClaimTypes.VehicleCoverages),         new VehicleCoveragesClaimTranslator() },
                    { ByClaimType(ClaimTypes.PreDeliveryInspection),    new PreDeliveryInspectionClaimTranslator() },
                    { ByClaimType(ClaimTypes.Policy),                   new PolicyClaimTranslator() },
                    { ByClaimType(ClaimTypes.MisBuilt),                 new MisBuiltClaimTranslator() },
                    { ByClaimType(ClaimTypes.ServiceParts),             new ServicePartsClaimTranslator() },
                    { ByClaimType(ClaimTypes.OverTheCounter),           new OverTheCounterClaimTranslator() },
                    { ByClaimType(ClaimTypes.Accessories),              new AccessoriesClaimTranslator() },
                    { ByClaimType(ClaimTypes.FieldServiceAction),       new FieldServiceActionClaimTranslator() },
                    { ByClaimType(ClaimTypes.FreeInspection),           new FreeInspectionClaimTranslator() },
                    { ByClaimType(ClaimTypes.TransitDamage),            new TransitDamageClaimTranslator() },
                    { ByClaimType(ClaimTypes.Fleet),                    new FleetClaimTranslator() },
                    { ByClaimType(ClaimTypes.ExtendedServiceContracts), new ExtendedServiceContractsClaimTranslator() },
                    { ByClaimType(ClaimTypes.RetailCoreReturn),         new RetailCoreReturnClaimTranslator() }
            });

        private static Func<SubmitClaimJobTranslatorArguments, bool> ByClaimType(string claimType) =>
            (args) => args?.Source != null && claimType.Equals(args.Source.Type);
    }
}
