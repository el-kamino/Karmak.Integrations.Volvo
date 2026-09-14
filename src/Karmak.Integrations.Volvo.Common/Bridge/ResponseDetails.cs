using System.Net;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    public class ResponseDetails
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public bool IsSuccessfulStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;
    }
}
