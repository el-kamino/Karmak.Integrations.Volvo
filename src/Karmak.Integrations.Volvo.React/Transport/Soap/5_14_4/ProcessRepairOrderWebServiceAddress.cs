
namespace Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4
{
    public class ProcessRepairOrderWebServiceAddress : SoapMessageAddress
    {
        public ProcessRepairOrderWebServiceAddress(string siteCode)
        {
            To = "urn:volvo/star/services/v1/GUDB/ProcessRepairOrderWebService/5.20";
            Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/PutMessage";
            TargetService = "ProcessRepairOrderWebService";
            TargetServiceVersion = "5.14.4";
            SiteCode = siteCode;
        }
    }
}