using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.PartsReturn;

public class PartsReturnConsumer : IConsumer<PartsReturnRequestReceived>
{
    private readonly IPartsReturnProcessor _partsReturnProcessor;
    private readonly IKarmakBlobClient _claimCheckClient;

    public PartsReturnConsumer(
        IPartsReturnProcessor partsReturnProcessor,
        [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
    {
        _partsReturnProcessor = partsReturnProcessor;
        _claimCheckClient = claimCheckClient;
    }

    public async Task Consume(ConsumeContext<PartsReturnRequestReceived> context)
    {
        var message = await _claimCheckClient.RetrieveAsync<PartsReturnRequest>(context.Message.BlobName);
        await _partsReturnProcessor.ProcessAsync(message);
    }
}
