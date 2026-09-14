using System.Linq;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class PartsInvoiceTypeMapper
    {
        private const string UNKNOWN_INVOICE_TYPE = "U";
        private const string E_COMMERCE_INVOICE_TYPE = "E";
        private const string WALK_IN_INVOICE_TYPE = "W";
        public static CodeType Map(string source, VolvoSettings settings)
        {
            return new CodeType
            {
                Value = GetSalesInvoiceTypeCode(source, settings)
            };
        }

        private static string GetSalesInvoiceTypeCode(string source, VolvoSettings settings)
        {
            if(IsWalkInInvoice(source, settings))
            {
                return WALK_IN_INVOICE_TYPE;
            }

            return IsECommerceInvoice(source, settings)
                ? E_COMMERCE_INVOICE_TYPE
                : UNKNOWN_INVOICE_TYPE;
        }

        private static bool IsWalkInInvoice(string source, VolvoSettings settings) =>
            settings.InterfaceOptions.WalkInPartsOrderSource.Contains(source);

        private static bool IsECommerceInvoice(string source, VolvoSettings settings) =>
            settings.InterfaceOptions.ECommercePartsOrderSource.Contains(source);
    }
}