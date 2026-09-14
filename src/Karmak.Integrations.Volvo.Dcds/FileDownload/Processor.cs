using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Dcds.Api;
using Karmak.Integrations.Volvo.Dcds.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Inbox;
using Karmak.Integrations.Volvo.Inbox.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Karmak.Integrations.Volvo.Dcds.FileDownload
{
    public class Processor : IDownloadFilesProcessor
    {
        public const string ActivitySourceName = "integrations.volvo.dcds.processor.download";

        private const string PLACEHOLDER_SOURCE_ID_REMOVE = "FM";

        private readonly IDcdsApiClient _dcdsClient;
        private readonly IInboxService _inboxService;
        private readonly IDcdsElkContextRetriever _elkContextRetriever;
        private readonly ILogger _logger;
        private readonly Counter<int> _filesProcessed;

        private static ActivitySource _activitySource = new ActivitySource(ActivitySourceName);

        public Processor(
            IDcdsApiClient dcdsClient, 
            IInboxService inboxService,
            IDcdsElkContextRetriever dealerLookup, 
            ILogger<Processor> logger,
            IMeterFactory meterFactory)
        {
            _dcdsClient = dcdsClient;
            _inboxService = inboxService;
            _elkContextRetriever = dealerLookup;
            _logger = logger;

            _filesProcessed = meterFactory
                .Create("integrations.volvo.dcds.processor.download")
                .CreateCounter<int>(MeterNames[0]);
        }

        public static string[] MeterNames = ["integrations.volvo.dcds.processor.download.files_processed"];

        public async Task<bool> ProcessAsync()
        {
            try
            {
                await ProcessAsyncImpl();
                return true;
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Volvo DCDS: Exception running Volvo DCDS process");
                return false;
            }
        }

        private async Task ProcessAsyncImpl()
        {
            using Activity? activity = _activitySource.StartActivity("dcds.download", ActivityKind.Server);

            List<DcdsFileInfo> dcdsFileInfoList = (await _dcdsClient.ListFile())!;
            List<VolvoShowFileResponse> inboxFiles = GetFileResponseFromDcdsFileInfo(dcdsFileInfoList);
            var fileGroups = inboxFiles.GroupBy(x => x.DealerId);

            foreach (var group in fileGroups)
            {
                using Activity? dealerActivity = _activitySource.StartActivity("dcds.download.dealer");
                List<VolvoShowFileResponse> files = group.ToList();

                dealerActivity?.AddTag("dcds.pa_code", group.Key);
                dealerActivity?.AddTag("dcds.file_count", files.Count);

                try
                {
                    string paCode = group.Key!;
                    ElkContext elkContext = await _elkContextRetriever.GetElkContextAsync(paCode);

                    if (elkContext == null)
                    {
                        throw new InvalidOperationException($"Found no dealer mapping for {paCode}");
                    }

                    foreach (var showFileResponse in files)
                    {
                        using Activity? fileActivity = _activitySource.StartActivity("dcds.download.dealer.file");
                        fileActivity?.AddTag("dcds.file_name", showFileResponse.Name);
                        fileActivity?.AddTag("dcds.file_type", showFileResponse.Type);

                        try
                        {
                            _logger.LogInformation($"Volvo DCDS: Starting file download for {showFileResponse.Name}");
                            showFileResponse.Detail = await _dcdsClient.DownloadFile(showFileResponse.Name);
                            FixedWidthFile transformedFile = TransformFileToFixWidth(showFileResponse);

                            await ImplicitElkContext.WithCurrentAsync(elkContext,
                                async () => await _inboxService.AddFixedWidthFileAsync(transformedFile));

                            _logger.LogInformation($"Volvo DCDS: Completed file download for {showFileResponse.Name}");
                            _filesProcessed.Add(1);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Volvo DCDS: Exception processing file {showFileResponse.Name} for dealer (PA Code) {paCode}");
                        }
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Volvo DCDS: Exception processing dealer (PA Code) {group.Key}");
                }
            }
        }

        private List<VolvoShowFileResponse> GetFileResponseFromDcdsFileInfo(List<DcdsFileInfo> dcdsFileInfoList)
        {
            List<VolvoShowFileResponse> showFileResponses = new List<VolvoShowFileResponse>();

            foreach (DcdsFileInfo fileInfo in dcdsFileInfoList)
            {
                //Tear filename apart  eg: "62d94587-a068-44fd-a9bf-1502d89ad8db_00261.F474" Remove everything to .F to get type, to underscore = dealerCode.
                var fileName = fileInfo.FileId;
                var dotLoc = fileName!.LastIndexOf(".");
                var underscoreLoc = fileName.LastIndexOf("_");

                var dealerCode = fileName.Substring(underscoreLoc + 1, dotLoc - underscoreLoc - 1);
                var fileType = fileName.Substring(dotLoc + 2);
                var fileTypeDescription = "unknown";
                if (FileTransferConstants.FILE_TYPES.Keys.Contains(fileType))
                {
                    fileTypeDescription = FileTransferConstants.FILE_TYPES[fileType];
                }

                showFileResponses.Add(new VolvoShowFileResponse
                {
                    Type = fileType,
                    Description = fileTypeDescription,
                    Name = fileName,
                    DocumentDateTime = DateTime.Now,
                    DealerId = dealerCode,
                    Detail = ""
                });
            }

            return showFileResponses;
        }
        
        private static FixedWidthFile TransformFileToFixWidth(VolvoShowFileResponse response)
        {
            var lines = GetLinesFromFile(response.Detail);
            var file = new FixedWidthFile
            {
                PACode = response.DealerId,
                FileName = response.Name,
                FileType = response.Type,
                FileDescription = response.Description,
                LineItems = lines,
                CreatedDate = response.DocumentDateTime ?? DateTime.MinValue,
                SourceId = PLACEHOLDER_SOURCE_ID_REMOVE,
            };

            return file;
        }

        private static IEnumerable<FixedWidthLine> GetLinesFromFile(string? file)
        {
            if (file == null || file.Length == 0)
            {
                return new List<FixedWidthLine>();
            }

            var lines = file.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            return lines.Select((v, i) => new FixedWidthLine { Contents = v, LineNumber = i });
        }
    }
}
