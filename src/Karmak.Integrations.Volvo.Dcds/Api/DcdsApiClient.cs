using Karmak.Integrations.Volvo.Dcds.Auth;
using Microsoft.Extensions.Options;
using System.Text;

namespace Karmak.Integrations.Volvo.Dcds.Api
{
    public class DcdsApiClient : IDcdsApiClient
    {
        private const int MaxLengthBytes = 10_000_000;

        private readonly string _clientId;
        private readonly Uri _inBoundApiUri;
        private readonly Uri _outBoundApiUri;
        private readonly HttpClient _httpClient;
        private readonly IOAuthTokenManager _tokenMgr;

        public enum RequestPath
        {
            GenFileId, // "fileID"
            ListFileId, // "listFileID"
            Download, // "downloadFile"
            Upload, // "uploadFile"
            Append_obsolete, // "appendFile"
            Copy_obsolete, // "copyFile"
            Commit, // "commitFile"
        }

        public DcdsApiClient(HttpClient httpClient, IOptions<DcdsApiClientOptions> options, IOAuthTokenManager tokenMgr)
        {
            ArgumentNullException.ThrowIfNull(options?.Value);
            var settings = options.Value;
            _inBoundApiUri = new Uri(settings.InBoundApiUri.EndsWith('/') ? settings.InBoundApiUri : $"{settings.InBoundApiUri}/");
            _outBoundApiUri = new Uri(settings.OutBoundApiUri.EndsWith('/') ? settings.OutBoundApiUri : $"{settings.OutBoundApiUri}/");
            _clientId = tokenMgr.ClientId;
            _tokenMgr = tokenMgr;
            _httpClient = httpClient;
        }

        private async Task<string?> GenerateFileId()
        {
            var request = await BuildBaseRequest(RequestPath.GenFileId);
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var result = await HttpContentExtractor.ExtractObject<DcdsResponseContent<DcdsFileInfo>>(response);
            return result?.Data?.FileId;
        }

        public async Task<List<DcdsFileInfo>?> ListFile()
        {
            return await ListFile("", "", "");
        }

        public async Task<List<DcdsFileInfo>?> ListFile(string dspId = "", string dealerCode = "", string docType = "")
        {
            var request = await BuildBaseRequest(RequestPath.ListFileId);
            request.Headers.Add("dspId", dspId);
            request.Headers.Add("dealerCode", dealerCode);
            request.Headers.Add("docType", docType);
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var result = await HttpContentExtractor.ExtractObject<DcdsResponseContent<List<DcdsFileInfo>>>(response);
            return result?.Data;
        }

        public async Task<string?> DownloadFile(string? fileId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileId);
            var request = await BuildBaseRequest(RequestPath.Download, fileId);
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            return await HttpContentExtractor.ExtractFile(response);
        }

        public async Task<string> UploadFile(ISendFileRequest sendFileRequest)
        {
            try
            {
                string? fileId = await GenerateFileId();

                if (string.IsNullOrWhiteSpace(fileId))
                {
                    throw new InvalidOperationException("Generated file id was null");
                }

                await Upload(RequestPath.Upload, fileId:fileId, sendFileRequest);
                await CommitFile(fileId, sendFileRequest.FileName);
                return fileId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Volvo DCDS: Failure while uploading file {sendFileRequest.FileName}.", ex);
            }
        }

        private async Task Upload(RequestPath reqPath,  string fileId, ISendFileRequest sendFileRequest)
        {
            // We really ought to check whether the file is over 10MB up in SendFileProcessor.ProcessAsync, split it if needed,
            // and make multiple calls, one for each piece.  But, I couldn't find any files we've sent larger than 256kb.  
            // Since these could increase 40x before becoming a problem, it is probably not an issue.  If it is bigger than  
            // 10 MB fail the upload
            if(sendFileRequest.Detail.Length > MaxLengthBytes)
            {
                throw new Exception($"Volvo DCDS: Failure while uploading file {sendFileRequest.FileName}. File exeeds 10MB limit");
            }

            using HttpRequestMessage request = await BuildBaseRequest(fileId: fileId, requestPath: reqPath, sendFileRequest: sendFileRequest);
            request.Content = new MultipartFormDataContent
            {
                { new StringContent(sendFileRequest.Detail), "file", fileId }
            };

            using HttpResponseMessage response = await _httpClient.SendAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                string context = $"Reason: {response.ReasonPhrase}.  CommitFile: {await response.Content.ReadAsStringAsync()}";
                throw new HttpRequestException(context, null, response.StatusCode);
            }
        }

        private async Task CommitFile(string fileId, string filename)
        {
            using HttpRequestMessage request = await BuildBaseRequest(RequestPath.Commit);
            request.Content = new MultipartFormDataContent
            {
                { new StringContent(fileId), "remotefileid" },
                { new StringContent(filename), "originalfilename" }
            };

            using HttpResponseMessage response = await _httpClient.SendAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                string context = $"Reason: {response.ReasonPhrase}.  CommitFile: {await response.Content.ReadAsStringAsync()}";
                throw new HttpRequestException(context, null, response.StatusCode);
            }
        }

        private async Task<HttpRequestMessage> BuildBaseRequest(RequestPath requestPath, string fileId = "", ISendFileRequest? sendFileRequest = null)
        {
            string countryCode = "", paCode = "", interfaceCode = "";
            if (sendFileRequest != null)
            {
                paCode = sendFileRequest.DealerId;
                interfaceCode = sendFileRequest.FileName.Substring(sendFileRequest.FileName.Length - 4, 4);
                countryCode = "USA";  // ! TODO: Uh, the actual access of the Volvo settings doesn't even happen in this solution.
                                      // We should either thread this along beside paCode or implement fetching settings from somewhere in here.
            }
            // ProcessFinancialStatement filename ends with F465  // FML these are set in Integrations.Volvo.DealerCommunications....
            // ProcessPartsReturn filename ends with F176

            HttpRequestMessage msg = requestPath switch
            {
                RequestPath.ListFileId => new HttpRequestMessage(HttpMethod.Get, new Uri(_outBoundApiUri, "listFileId")),
                RequestPath.Download => new HttpRequestMessage(HttpMethod.Get, new Uri(_outBoundApiUri, $"downloadFile/?remote_file_id={fileId}")),

                RequestPath.GenFileId => new HttpRequestMessage(HttpMethod.Get, new Uri(_inBoundApiUri, "fileID")),
                RequestPath.Upload => new HttpRequestMessage(HttpMethod.Post, new Uri(_inBoundApiUri,
                            $"uploadFile/?remote_file_id={fileId}&countryCode={countryCode}&dealerCode={paCode}&interfaceCode={interfaceCode}")),
                RequestPath.Commit => new HttpRequestMessage(HttpMethod.Post, new Uri(_inBoundApiUri, "commitFile")),
                _ => throw new NotImplementedException(),
            };

            switch (requestPath)
            {
                case RequestPath.GenFileId:
                case RequestPath.Upload:
                case RequestPath.Commit:
                    msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenMgr.GetInboundToken());
                    break;
                case RequestPath.ListFileId:
                case RequestPath.Download:
                    msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenMgr.GetOutboundToken());
                    break;
            }
            msg.Headers.Add("ClientID", _clientId);
            msg.Headers.TryAddWithoutValidation("Accept-Encoding", "application/gzip");
            msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            if (requestPath == RequestPath.Upload)
            {
                msg.Headers.TryAddWithoutValidation("countryCode", countryCode);
                msg.Headers.TryAddWithoutValidation("interfaceCode", interfaceCode);
                msg.Headers.TryAddWithoutValidation("dealerCode", paCode);
            }
            return msg;
        }
    }
}
