namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public class ProcessServiceAppointmentWebsServiceAddress : SoapMessageAddress
    {
        public ProcessServiceAppointmentWebsServiceAddress(string siteCode)
        {
            To = "urn:volvo/star/services/v1/GUDB/ProcessServiceAppointmentWebService/5.10";
            Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage";
            TargetService = "ProcessServiceAppointmentWebService";
            TargetServiceVersion = "5.10.2";
            SiteCode = siteCode;
        }
    }
}