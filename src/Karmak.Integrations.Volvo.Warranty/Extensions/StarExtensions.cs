using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Extensions
{
    public static class StarExtensions
    {
        public static string ExtractServiceId(this ShowServiceProcessingAdvisoryType showServiceProcessingAdvisory)
        {
            return showServiceProcessingAdvisory?.ApplicationArea?.Sender?.ServiceID?.Value;
        }
    }
}
