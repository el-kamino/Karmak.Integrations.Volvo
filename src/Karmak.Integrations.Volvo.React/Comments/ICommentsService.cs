using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Comments
{
    internal interface ICommentsService
    {
        ReceiveCommentsBody BuildCommentsBody(VolvoSettings settings, RepairOrderSnapshot ro);
        string GetCommentsHash(List<CommentsItem> comments);
        Dictionary<string, string> GetDetailedMetadata(ReceiveCommentsBody commentsBody, IDictionary<string, string> roMetadata);
        Task<string> GetVolvoOAuthTokenAsync();
        Task<bool> SendCommentsToVolvoAsync(ReceiveCommentsBody body, string[] reactEmails, Dictionary<string, string> roMetadata);
        bool TokenIsExpired(string token);
    }
}