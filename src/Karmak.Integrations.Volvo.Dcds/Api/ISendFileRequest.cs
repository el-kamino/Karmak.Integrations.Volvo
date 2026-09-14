namespace Karmak.Integrations.Volvo.Dcds.Api
{
    public interface ISendFileRequest
    {
        string SendType { get; set; }
        string SendTypeVersion { get; set; }
        string FileName { get; set; }
        string DealerId { get; set; }
        string Detail { get; set; }
    }
}