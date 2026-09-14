using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React
{
    public interface IVolvoExtendedLoggingService
    {
        Task LogInboundDto(object entity, Dictionary<string, string> entityMetadata = null);
        Task LogOutBoundMessage(string message, Dictionary<string, string> entityMetadata = null);
        Task LogOutBoundMessages(string[] messages, Dictionary<string, string> entityMetadata = null);
    }
}