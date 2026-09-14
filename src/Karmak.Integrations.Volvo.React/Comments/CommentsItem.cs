using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Comments
{
    public static class DocumentType
    {
        public const string RepairOrder = "RO";
    }

    public class ReceiveCommentsBody
    {
        public CommentsSender sender;
        public CommentsPayload payload;
    }

    public class CommentsSender
    {
        public string dspId;
        public string clientId;
        public string siteCode;
        public string resource;
        public string msgId;
        public string countryCode;
    }

    public class CommentsPayload
    {
        public string documentType;
        public string paCode;
        //public string subCode; Not used
        public DateTime? documentDatetime;
        public DateTime? openDate;
        public string documentId;
        public string vin;
        public List<CommentsItem> comments;
    }

    public class CommentsItem
    {
        public string commentType;
        public string comment;
        //public string starsId; Not used
        public string localId;
        public string lineCode;
    }
}
