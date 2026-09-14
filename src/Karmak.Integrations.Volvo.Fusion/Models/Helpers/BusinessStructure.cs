using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.Helpers;

public static class BusinessStructure
{
    private const string Individual = "Individual";

    public static bool IsIndividual(FusionCustomer customer)
    {
        return string.Equals(Individual, customer.BusinessStructure, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsOrganization(FusionCustomer customer)
    {
        return !IsIndividual(customer);
    }
}
