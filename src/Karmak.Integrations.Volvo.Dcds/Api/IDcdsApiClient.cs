namespace Karmak.Integrations.Volvo.Dcds.Api
{
    public interface IDcdsApiClient
    {
        Task<string> UploadFile(ISendFileRequest sendFileRequest);
        Task<List<DcdsFileInfo>?> ListFile();
        Task<string?> DownloadFile(string? fileId);
    }
}
