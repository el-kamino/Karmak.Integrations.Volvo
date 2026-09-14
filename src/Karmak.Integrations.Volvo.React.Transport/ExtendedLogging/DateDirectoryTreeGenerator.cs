using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Elk.Identity;

namespace Karmak.Integrations.Volvo.React.Transport.ExtendedLogging
{
    public class DateDirectoryTreeGenerator : IExtendedLogDirectoryGenerator
    {
        public string GetDirectory(string baseDirectory)
        {
            var elkContext = ImplicitElkContext.Current;
            var instanceId = elkContext?.ApplicationContext?.Instance?.ToString() ?? "UnknownInstance";
            var branchId = elkContext?.ApplicationContext?.Branch?.ToString() ?? "UnknownBranch";
            return $"{baseDirectory}/{DateTime.UtcNow:yyyy/MM-MMMM/dd-dddd}/{instanceId}/{branchId}";
        }
    }
}
