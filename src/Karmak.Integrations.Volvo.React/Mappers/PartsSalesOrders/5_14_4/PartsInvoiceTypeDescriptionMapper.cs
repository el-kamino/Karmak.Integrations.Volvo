using System.Linq;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4
{
    static class PartsInvoiceTypeDescriptionMapper
    {
        private const string UNKNOWN_INVOICE_TYPE = "Unknown";
        private const string E_COMMERCE_INVOICE_TYPE = "eCommerce";
        private const string WALK_IN_INVOICE_TYPE = "Walk-in";
        public static TextType Map(string source, VolvoSettings settings)
        {
            return new TextType
            {
                Value = GetSalesInvoiceTypeDescription(source, settings)
            };
        }

        private static string GetSalesInvoiceTypeDescription(string source, VolvoSettings settings)
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