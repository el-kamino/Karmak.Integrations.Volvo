
using Karmak.Integrations.Volvo.Dcds.Contracts;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.PartsReturn;

public interface IPartsReturnProcessor
{
    Task ProcessAsync(PartsReturnRequest message);
}