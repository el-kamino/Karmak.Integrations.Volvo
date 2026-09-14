using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Comments
{
    internal interface IEmailSender
    {
        Task Send(List<string> toList, string body);
        Task Send(List<string> toList, string body, string subject);
    }
}