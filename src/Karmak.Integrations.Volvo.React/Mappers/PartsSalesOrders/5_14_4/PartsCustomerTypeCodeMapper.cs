using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;

namespace Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4
{
    static class PartsCustomerTypeCodeMapper
    {
        public static CodeType Map(string partsCustomerType)
        {
            return new CodeType
            {
                Value = string.IsNullOrWhiteSpace(partsCustomerType) ? "U" : partsCustomerType
            };
        }
    }
}