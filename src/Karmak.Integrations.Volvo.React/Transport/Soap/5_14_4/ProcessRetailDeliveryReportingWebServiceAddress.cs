using Karmak.Integrations.Volvo.React.Transport.Soap;

namespace Elk.Integrations.Volvo.Core.Transport.Soap.V5_14_4
{
    public class ProcessRetailDeliveryReportingWebServiceAddress : SoapMessageAddress
    {
        public ProcessRetailDeliveryReportingWebServiceAddress(string siteCode)
        {
            To = "urn:volvo/star/services/v1/GUDB/ProcessRetailDeliveryReportingWebService/5.20";
            Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage";
            TargetService = "ProcessRetailDeliveryReportingWebService";
            TargetServiceVersion = "5.10.2";
            SiteCode = siteCode;
        }
    }
}