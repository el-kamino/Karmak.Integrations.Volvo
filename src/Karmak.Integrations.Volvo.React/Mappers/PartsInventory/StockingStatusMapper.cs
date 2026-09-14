using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;

namespace Karmak.Integrations.Volvo.React.Mappers.PartsInventory
{
    public static class StockingStatusMapper
    {
        public static CodeType Map(InventoryPart part)
        {
            return Map(part.StockingStatus);
        }

        public static CodeType Map(StockingStatus status)
        {
            switch (status)
            {
                case StockingStatus.Stocked:
                    return new CodeType
                    {
                        Value = "Y"
                    };
                case StockingStatus.NonStocked:
                    return new CodeType
                    {
                        Value = "N"
                    };
                default:
                    return new CodeType
                    {
                        Value = "U"
                    };
            }
        }
    }
}