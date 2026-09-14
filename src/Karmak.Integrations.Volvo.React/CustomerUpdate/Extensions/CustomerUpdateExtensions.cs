using System;
using System.Collections.Generic;
using System.Text;
using Karmak.Integrations.Volvo.React.Validators.CustomerUpdate;
using CustomerInfo = Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions
{
    public static class CustomerUpdateExtensions
    {
        public static bool IsValid(this CustomerInfo record, out string errors) =>
            CustomerUpdateValidator.IsValid(record, out errors);

        public static bool HasVolvoPassRewards(this CustomerInfo record) =>
            record.VolvoPassRewardID > 0;

        public static bool HasNoVin(this CustomerInfo record) =>
            !(record.CurrentVINsIsSet() || record.HasAddedVIN() || record.HasRemovedVIN());

        public static bool HasAddedVIN(this CustomerInfo record) =>
            !string.IsNullOrWhiteSpace(record.AddedVIN);

        public static bool HasRemovedVIN(this CustomerInfo record) =>
            !string.IsNullOrWhiteSpace(record.RemovedVIN);

        public static bool CurrentVINsIsSet(this CustomerInfo record) =>
            record.CurrentVINs?.Count > 0;
    }
}
