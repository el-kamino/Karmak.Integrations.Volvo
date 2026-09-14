using Karmak.Integrations.Volvo.Dcds.Api;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload
{
    public interface ISendFileProcessor
    {
        Task ProcessAsync<TRequest>(TRequest sendRequest) where TRequest : ISendFileRequest;
    }
}