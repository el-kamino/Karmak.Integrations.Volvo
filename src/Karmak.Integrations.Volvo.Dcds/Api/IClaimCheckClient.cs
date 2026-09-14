namespace Karmak.Integrations.Volvo.Dcds.Api
{
    public interface IClaimCheckClient
    {
        Task<string> Retrieve(Uri blobUri);
    }
}
