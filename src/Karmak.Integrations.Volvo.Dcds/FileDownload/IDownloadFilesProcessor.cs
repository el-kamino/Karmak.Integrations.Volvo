namespace Karmak.Integrations.Volvo.Dcds.FileDownload
{
    public interface IDownloadFilesProcessor
    {
        Task<bool> ProcessAsync();
    }
}