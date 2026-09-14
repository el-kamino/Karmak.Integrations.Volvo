using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.FinancialStatement;

internal class FinancialStatementReceivedConsumer : IConsumer<FinancialStatementRequestReceived>
{
    private readonly IFinancialStatementReceivedProcessor _financialStatementReceivedProcessor;
    private readonly IKarmakBlobClient _claimCheckClient;

    public FinancialStatementReceivedConsumer(
        IFinancialStatementReceivedProcessor financialStatementReceivedProcessor,
        [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
    {
        _financialStatementReceivedProcessor = financialStatementReceivedProcessor;
        _claimCheckClient = claimCheckClient;
    }

    public async Task Consume(ConsumeContext<FinancialStatementRequestReceived> context)
    {
        var message = await _claimCheckClient.RetrieveAsync<FinancialStatementRequest>(context.Message.BlobName);
        await _financialStatementReceivedProcessor.ProcessAsync(message);
    }
}
