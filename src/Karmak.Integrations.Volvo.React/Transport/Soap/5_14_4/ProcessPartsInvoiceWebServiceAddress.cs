
namespace Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;

public class ProcessPartsInvoiceWebServiceAddress : SoapMessageAddress
{
    public ProcessPartsInvoiceWebServiceAddress(string siteCode)
    {
        To = "urn:volvo/star/services/v1/GUDB/ProcessPartsInvoiceWebService/5.20";
        Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage";
        TargetService = "ProcessPartsInvoiceWebService";
        TargetServiceVersion = "5.14.4";
        SiteCode = siteCode;
    }
}