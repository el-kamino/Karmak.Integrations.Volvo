
namespace Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;

public class ProcessPartsInventoryWebServiceAddress : SoapMessageAddress
{
    public ProcessPartsInventoryWebServiceAddress(string siteCode)
    {
        To = "urn:volvo/star/services/v1/GUDB/ProcessPartsInventoryWebService/5.20";
        Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage";
        TargetService = "ProcessPartsInventoryWebService";
        TargetServiceVersion = "5.10.2";
        SiteCode = siteCode;
    }
}