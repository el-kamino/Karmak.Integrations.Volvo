namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public interface ISoapRequestFactory
    {
        SoapEnvelope CreateProcessRequest<T>(SoapMessageAddress address, T payload);
        SoapEnvelope CreatePutRequest<T>(SoapMessageAddress address, T payload);
    }
}