using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    public static class MappingConstants
    {
        public static readonly IReadOnlyDictionary<string, string> StatusDescriptions =
            new Dictionary<string, string>
            {
                ["01"] = "Paid",
                ["AE"] = "Approved - ESP Repairs",
                ["A"] = "Approved - Other Repairs"
            };
        public const string DefaultStatusDescription = "Other";
    }
}
