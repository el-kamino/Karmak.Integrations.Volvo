using System.Threading.Tasks;
using System.Xml;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IVolvoPushUpdateService
    {
        Task<(bool success, XmlDocument response)> HandleUpdate(XmlDocument request);
    }
}
