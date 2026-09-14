using FluentValidation;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Karmak.Integrations.Volvo.Inbox.Validators;
using Karmak.Integrations.Volvo.Inbox.Validators.Blob;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Inbox;

public class InboxService : IInboxService
{
    private readonly IInboxRepository _metaDataStore;
    private readonly IStorageClient _contentStorage;
    private readonly ILogger _logger;

    public InboxService(IInboxRepository metadataStorage, IStorageClient contentStorage, ILogger<InboxService> logger)
    {
        _metaDataStore = metadataStorage;
        _contentStorage = contentStorage;
        _logger = logger;
    }

    public async Task<MetaMessageEnvelope> AddFixedWidthFileAsync(FixedWidthFile file)
    {
        ThrowOnNullArgument(file, "file cannot be null");

        var validationResults = new FixedWidthFileValidator().Validate(file);
        if (!validationResults.IsValid)
            throw new ValidationException(validationResults.Errors);

        string blobName = GetBlobName(file);

        var uploadRequest = new BlobUploadRequest<List<FixedWidthLine>>(file.LineItems.ToList())
        {
            BlobName = blobName,
            OriginalFileName = file.FileName,
            FileType = file.FileType,
        };

        string contentUri = await _contentStorage.UploadAsync(uploadRequest);

        MetaMessageEnvelope savedFile = await _metaDataStore.UpsertAsync(
                MetaMessageEnvelopeMapper.Map(file, GetAccountNumberAndBranchId(), contentUri));

        return savedFile;
    }

    public async Task<PagedResult<InboxMessage>> GetAllInboxMessagesAsync(PagingArguments args)
    {
        ThrowOnNullArgument(args, "Paging Arguments are required to get messages");

        var validationResults = new PagingArgumentsValidator().Validate(args);
        if (!validationResults.IsValid)
            throw new ValidationException(validationResults.Errors);

        var accountAndBranchId = GetAccountNumberAndBranchId();
        return await _metaDataStore.GetAllAsync(accountAndBranchId, args);
    }

    public async Task<InboxMessageContents> GetMessageContentsAsync(string messageId)
    {
        ThrowOnNullOrWhiteSpaceArgument(messageId, "Id is required to fetch file contents");

        var (accountNumber, _) = GetAccountNumberAndBranchId();
        var message = await _metaDataStore.GetByIdAsync(accountNumber, messageId);

        var response = new InboxMessageContents { Id = messageId };

        if (string.IsNullOrWhiteSpace(message.ContentUri))
        {
            _logger.LogWarning($"No message contents for message id {messageId}");
            response.Lines = new List<InboxMessageContentLine>();
        }
        else
        {
            var lines = await _contentStorage.DownloadAsync<List<FixedWidthLine>>(message.ContentUri);

            response.Lines = lines.Select(l =>
                new InboxMessageContentLine
                {
                    LineNumber = l.LineNumber,
                    Contents = l.Contents
                });
        }

        return response;
    }

    public async Task<List<InboxMessageContents>> GetBulkMessageContentsAsync(BulkRetrieveMessageContentsArgs args)
    {
        ThrowOnNullArgument(args, "BulkRetrieveMessageContentsArgs are required to retrieve message contents");

        var validationResults = new BulkRetrieveMessageContentsArgsValidator().Validate(args);
        if (!validationResults.IsValid)
            throw new ValidationException(validationResults.Errors);

        var response = new List<InboxMessageContents>();
        var (accountNumber, branchId) = GetAccountNumberAndBranchId();
        var contentPointers =
            await _metaDataStore.GetMessageContentPointersAsync(accountNumber, branchId,
                args.MessageIds);

        foreach (var contentPointer in contentPointers)
        {
            var content = new InboxMessageContents
            {
                Id = contentPointer.MessageId,
                Lines = new List<InboxMessageContentLine>()
            };

            await RetrieveMessageContents(contentPointer, content);

            response.Add(content);
        }

        return response;
    }

    public async Task<InboxMessage> UpdateInboxMessageAsync(InboxMessageUpdateArguments args)
    {
        ThrowOnNullArgument(args, "InboxMessageUpdateArguments are required to save updates");

        var validationResults = new InboxMessageUpdateArgumentsValidator().Validate(args);
        if (!validationResults.IsValid)
            throw new ValidationException(validationResults.Errors);

        var (accountNumber, _) = GetAccountNumberAndBranchId();
        args.AccountNumber = accountNumber;

        var response = await _metaDataStore.UpdateAsync(args);
        return response;
    }

    public async Task<List<InboxMessage>> UpdateInboxMessagesAsync(BulkMessageUpdateArguments args)
    {
        ThrowOnNullArgument(args, "BulkMessageUpdateArguments are required to save updates");

        var validationResults = new BulkMessageUpdateArgumentsValidator().Validate(args);
        if (!validationResults.IsValid)
            throw new ValidationException(validationResults.Errors);

        var (accountNumber, branchId) = GetAccountNumberAndBranchId();

        var response = await _metaDataStore.BulkUpdateAsync(accountNumber, branchId, args);
        return response;
    }

    private (string, string) GetAccountNumberAndBranchId()
    {
        var accountNumber = ImplicitElkContext.Current?.ApplicationContext?.Instance == null
            ? ImplicitElkContext.Current.Identity.Account.ToString()
            : ImplicitElkContext.Current.ApplicationContext.Instance.Value.ToString();

        return (accountNumber, ImplicitElkContext.Current.ApplicationContext.Branch?.ToString());
    }

    private async Task RetrieveMessageContents(MessageContentPointer contentPointer, InboxMessageContents content)
    {
        if (!string.IsNullOrWhiteSpace(contentPointer.ContentUri))
            try
            {
                var lines =
                    await _contentStorage.DownloadAsync<List<FixedWidthLine>>(contentPointer
                        .ContentUri);
                content.Lines = lines.Select(l => new InboxMessageContentLine
                {
                    LineNumber = l.LineNumber,
                    Contents = l.Contents
                });
            }
            catch (Exception e)
            {
                var ex = new Exception(
                    $"Failed to retrieve file contents.  Message id: {contentPointer.MessageId}.  Url: {contentPointer.ContentUri}.", e);

                throw ex;
            }
    }

    private static void ThrowOnNullArgument<T>(T argument, string message)
    {
        if (argument == null)
        {
            throw new ArgumentException(message);
        }
    }

    private static void ThrowOnNullOrWhiteSpaceArgument(string argument, string message)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException(argument, message);
        }
    }

    private static string GetBlobName(FixedWidthFile file) =>
       $"{file.PACode}/{file.FileName}_{Guid.NewGuid().ToString("d")}";
}