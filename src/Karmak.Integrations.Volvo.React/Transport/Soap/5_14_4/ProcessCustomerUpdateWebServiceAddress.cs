namespace Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;

public class ProcessCustomerUpdateWebServiceAddress : SoapMessageAddress
{
    public ProcessCustomerUpdateWebServiceAddress(string siteCode)
    {
        To = "urn:volvo/star/services/v1/GUDB/ProcessCustomerInformationWebService/5.20";
        Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage";
        TargetService = "ProcessCustomerInformationWebService";
        TargetServiceVersion = "5.10.2";
        SiteCode = siteCode;
    }
}