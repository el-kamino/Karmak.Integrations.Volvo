using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Comments
{
    internal class CommentsService : ICommentsService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tokenUri;
        private readonly string _resource;
        private readonly string _commentsBaseUri;
        private readonly string _commentsReceivePath;
        private readonly IEmailSender _emailSender;
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly IVolvoExtendedLoggingService _extendedLoggingSvc;

        private string _token = null;

        public CommentsService(
            HttpClient httpClient,
            IEmailSender emailSender,
            ILogger<CommentsService> logger,
            IVolvoExtendedLoggingService extendedLoggingService,
            IOptions<CommentsServiceOptions> options)
        {
            _emailSender = emailSender;
            _logger = logger;
            _extendedLoggingSvc = extendedLoggingService;
            _client = httpClient;

            _clientId = options.Value.ClientId;
            _clientSecret = options.Value.ClientSecret;
            _tokenUri = options.Value.TokenUri;
            _resource = options.Value.Resource;
            _commentsBaseUri = options.Value.CommentsBaseUrl;
            _commentsReceivePath = options.Value.CommentsReceivePath;
        }

        public async Task<bool> SendCommentsToVolvoAsync(ReceiveCommentsBody body, string[] reactEmails, Dictionary<string, string> roMetadata)
        {
            if (_commentsBaseUri == null || _commentsReceivePath == null)
            {
                _logger.LogInformationWithMetadata("Comment URI or Path empty.", roMetadata);
                return false;
            }

            if (body.payload.comments.Count == 0)
            {
                _logger.LogInformationWithMetadata("No comments to send.", roMetadata);
                return false;
            }

            _logger.LogInformationWithMetadata($"Begin processing {body.payload.comments.Count} comments.", roMetadata);
            bool isSuccess = await ProcessRequest(body, reactEmails?.ToList(), roMetadata);
            return isSuccess;
        }

        public string GetCommentsHash(List<CommentsItem> comments)
        {
            if (comments?.Count > 0)
            {
                using (var hasher = MD5.Create())
                {
                    string json = JsonConvert.SerializeObject(comments);
                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    bytes = hasher.ComputeHash(bytes);
                    return Encoding.ASCII.GetString(bytes);
                }
            }
            return null;
        }

        private async Task<HttpRequestMessage> BuildRequest(ReceiveCommentsBody body, Dictionary<string, string> roMetadata)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_commentsBaseUri), _commentsReceivePath));

            if (_token is null)
            {
                _token = await GetVolvoOAuthTokenAsync();
                _logger.LogInformationWithMetadata($"Token retreived.", roMetadata);
            }
            if (TokenIsExpired(_token))
            {
                await RenewToken(roMetadata);
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");

            string jsonRequestBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            await _extendedLoggingSvc.LogOutBoundMessage(jsonRequestBody, roMetadata);
            request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            return request;
        }

        public bool TokenIsExpired(string token)
        {
            try
            {
                JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                if (jwt.ValidTo < DateTime.UtcNow)
                    return true; // expired
            }
            catch (Exception e)
            {
                throw new AggregateException("Comments Service token broken!", e);
            }
            return false; // not expired
        }

        private const string BadRequestSubject = "Failed to Send Comments (Bad Request)";
        private string BadRequestMessage(string docType, string docId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Comments failed to send for {docType} with ID of {docId}.");
            sb.AppendLine("Confirm the request is properly configured and retry.");
            sb.AppendLine("If a retry fails, contact your Volvo support representative.");
            return sb.ToString();
        }

        private const string MethodNotAllowedSubject = "Failed to Send Comments (Method Not Allowed)";
        private string MethodNotAllowedMessage(string docType, string docId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Comments failed to send for {docType} with ID of {docId}.");
            sb.AppendLine("Confirm correct REST method is configured and retry.");
            sb.AppendLine("If a retry fails, contact your Volvo support representative.");
            return sb.ToString();
        }

        private async Task<bool> ProcessRequest(ReceiveCommentsBody body, List<string> emailList, Dictionary<string, string> roMetadata, bool isReattempt = false)
        {
            HttpResponseMessage response = await TransmitCommentsBody(body, roMetadata);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
//#if DEBUG
//                    if (emailList?.Count > 0)
//                        _emailSender.Send(emailList,
//                            body: "Comments were transmitted to Volvo & an HTTP response code of 200 (OK) was received",
//                            subject: "DEBUG: Comments Transmitted Successfully");
//#endif
                    return true;

                case HttpStatusCode.BadRequest:
                    _logger.LogInformationWithMetadata($"Received 400 Status Code (Bad Request)", roMetadata);
                    if (emailList?.Count > 0)
                    {
                        string msg = BadRequestMessage(body.payload.documentType, body.payload.documentId);
                        _emailSender.Send(emailList, msg, BadRequestSubject);
                    }
                    return false;

                case HttpStatusCode.Unauthorized:
                    if (TokenIsExpired(_token))
                    {
                        if (!isReattempt)
                        {
                            // get new token and retry ONCE
                            await RenewToken(roMetadata);
                            return await ProcessRequest(body, emailList, roMetadata, isReattempt: true);
                        }
                        _logger.LogInformationWithMetadata($"Token renewal failed", roMetadata);
                        throw new HttpRequestException($"Token renewal failed & not currently handled");
                    }
                    // Token is not expired, something else is wrong.
                    var metadata = GetDetailedMetadata(body, roMetadata);
                    metadata["response.ReasonPhrase"] = response.ReasonPhrase;
                    _logger.LogInformationWithMetadata($"Received {response.StatusCode} Status Code with a non-expired token", metadata);
                    throw new HttpRequestException($"{response.StatusCode} Error: not currently handled");

                case HttpStatusCode.MethodNotAllowed:
                    _logger.LogInformationWithMetadata($"Received 405 Status Code (Method Not Allowed)", roMetadata);
                    if (emailList?.Count > 0)
                    {
                        string msg = MethodNotAllowedMessage(body.payload.documentType, body.payload.documentId);
                        _emailSender.Send(emailList, msg, MethodNotAllowedSubject);
                    }
                    return false;

                default:
                    // unexpected HTTP response code
                    _logger.LogInformationWithMetadata($"Received {response.StatusCode} Status Code{(isReattempt ? " after token renewal" : string.Empty)}", roMetadata);
                    throw new HttpRequestException($"{response.StatusCode} Error received{(isReattempt ? " after token renewal" : string.Empty)} & not currently handled");
            }
        }

        private async Task<HttpResponseMessage> TransmitCommentsBody(ReceiveCommentsBody body, Dictionary<string, string> roMetadata)
        {
            HttpRequestMessage request = await BuildRequest(body, roMetadata);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return response;
        }

        private async Task RenewToken(Dictionary<string, string> roMetadata)
        {
            _logger.LogInformationWithMetadata($"Token expired; retreiving new token...", roMetadata);
            _token = await GetVolvoOAuthTokenAsync();
            _logger.LogInformationWithMetadata($"Token retreived.", roMetadata);
        }

        public async Task<string> GetVolvoOAuthTokenAsync()
        {
            var tokenResponse = await GetTokenResponse(new CancellationToken());
            return tokenResponse?.AccessToken;
        }

        private async Task<TokenResponse> GetTokenResponse(CancellationToken cancellationToken)
        {
            if (_tokenUri == null)
                return new TokenResponse();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["resource"] = _resource
            });

            using (var response = await _client.PostAsync(_tokenUri, content, cancellationToken))
            using (var ensureResponse = response.EnsureSuccessStatusCode())
            using (var responseStream = await ensureResponse.Content.ReadAsStreamAsync())
            using (var responseReader = new StreamReader(responseStream))
            {
                var serializer = new JsonSerializer();
                return (TokenResponse)serializer.Deserialize(responseReader, typeof(TokenResponse));
            }
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; private set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; private set; }
        }

        public ReceiveCommentsBody BuildCommentsBody(VolvoSettings settings, RepairOrderSnapshot ro)
        {
            var countryCode = settings?.RegionSettings?.CountryCode ?? "";
            var paCode = settings?.InterfaceOptions?.PaCode ?? "";
            return new ReceiveCommentsBody()
            {
                sender = new CommentsSender()
                {
                    dspId = settings?.DealerServiceProviderSettings?.VendorIdentifier ?? "",
                    clientId = _clientId,
                    siteCode = countryCode + paCode,
                    resource = "urn:Volvo/FNOP/ReceiveComments/V1",
                    msgId = Guid.NewGuid().ToString(),
                    countryCode = countryCode
                },
                payload = new CommentsPayload()
                {
                    documentType = DocumentType.RepairOrder,
                    paCode = paCode,
                    documentDatetime = ro.SnapshotSequenceNumberDateTime.ToLocalTime().WithoutMilliseconds(),
                    openDate = ro.OpenDate.GetValueOrDefault().ToLocalTime().WithoutMilliseconds(),
                    documentId = ro.RepairOrderNumber,
                    vin = ro.Vehicle.VIN,
                    comments = BuildCommentsList(settings, ro)
                }
            };
        }

        private List<CommentsItem> BuildCommentsList(VolvoSettings settings, RepairOrderSnapshot ro)
        {
            var comments = new List<CommentsItem>();
            int awaTaskCount = 0;
            for (int i = 0; i < ro.Tasks.Count; i++)
            {
                var t = ro.Tasks[i];
                var commentItem = new CommentsItem()
                {
                    localId = t.LaborEntries.IsEmpty() ? null : t.LaborEntries[0].TechnicianNumber.ToString(),
                    lineCode = "00"
                };
                if (settings.InterfaceOptions.AwaCustomers.NotNullAndContains(t.AlternateBillingCustomerKey))
                {
                    awaTaskCount++;
                    commentItem.lineCode = awaTaskCount.ToString("00");
                }

                if (!string.IsNullOrWhiteSpace(t.ComplaintNotes))
                {
                    commentItem.comment = t.ComplaintNotes;
                    commentItem.commentType = "Customer Complaint";
                    comments.Add(GetNewCommentsItem(commentItem));
                    commentItem.commentType = "Service Advisor Comment";
                    comments.Add(GetNewCommentsItem(commentItem));
                }

                if (!string.IsNullOrWhiteSpace(t.TechnicianNotes))
                {
                    commentItem.comment = t.TechnicianNotes;
                    commentItem.commentType = "Technician Comment";
                    comments.Add(GetNewCommentsItem(commentItem));
                }

                if (!string.IsNullOrWhiteSpace(t.RepairTypeDescription))
                {
                    commentItem.comment = t.RepairTypeDescription;
                    commentItem.commentType = "Labor Operation Description";
                    comments.Add(GetNewCommentsItem(commentItem));
                }
            }

            return comments;
        }

        /// <summary>
        /// Bolt-on copy constructor
        /// </summary>
        /// <param name="commentsItem"></param>
        /// <returns>A new CommentsItem based on the input</returns>
        private CommentsItem GetNewCommentsItem(CommentsItem commentsItem)
        {
            return new CommentsItem
            {
                commentType = commentsItem.commentType,
                comment = commentsItem.comment,
                localId = commentsItem.localId,
                lineCode = commentsItem.lineCode,
            };
        }

        public Dictionary<string, string> GetDetailedMetadata(ReceiveCommentsBody commentsBody, IDictionary<string, string> roMetadata)
        {
            var detailedMetadata = new Dictionary<string, string>(roMetadata);
            detailedMetadata["_tokenUri"] = _tokenUri;
            detailedMetadata["_clientId"] = _clientId;
            detailedMetadata["_resource"] = _resource;
            detailedMetadata["_commentsBaseUri"] = _commentsBaseUri;
            detailedMetadata["_commentsReceivePath"] = _commentsReceivePath;

            detailedMetadata["commentsBody.sender.dspId"] = commentsBody.sender.dspId;
            detailedMetadata["commentsBody.sender.clientId"] = commentsBody.sender.clientId;
            detailedMetadata["commentsBody.sender.siteCode"] = commentsBody.sender.siteCode;
            detailedMetadata["commentsBody.sender.resource"] = commentsBody.sender.resource;
            detailedMetadata["commentsBody.sender.msgId"] = commentsBody.sender.msgId;
            detailedMetadata["commentsBody.sender.countryCode"] = commentsBody.sender.countryCode;

            detailedMetadata["commentsBody.payload.documentType"] = commentsBody.payload.documentType;
            detailedMetadata["commentsBody.payload.paCode"] = commentsBody.payload.paCode;
            detailedMetadata["commentsBody.payload.documentDatetime"] = commentsBody.payload.documentDatetime?.ToString() ?? "";
            detailedMetadata["commentsBody.payload.openDate"] = commentsBody.payload.openDate?.ToString() ?? "";
            detailedMetadata["commentsBody.payload.documentId"] = commentsBody.payload.documentId;
            detailedMetadata["commentsBody.payload.vin"] = commentsBody.payload.vin;

            return detailedMetadata;
        }
    }
}